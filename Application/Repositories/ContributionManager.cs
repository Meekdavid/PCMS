using Application.Interfaces.Business;
using Application.Interfaces.Database;
using Application.Interfaces.General;
using AutoMapper;
using AutoMapper.Execution;
using Common.ConfigurationSettings;
using Common.DTOs.Requests;
using Common.DTOs.Responses;
using Common.Models;
using Common.Services;
using Core.Results;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Persistence.DBModels;
using Persistence.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using static Common.Literals.StringLiterals;

namespace Application.Repositories
{
    public class ContributionManager : IContributionManager
    {
        private readonly IContributionRepository _contributionDal;
        private readonly IMemberRepository _memberDal;
        private readonly ITransactionManager _transactionManager;
        private readonly IAccountManager _accountManager;
        private readonly IAccountRepository _accountDal;
        private readonly IFileManager _fileManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITransactionRepository _transactionDal;
        private readonly ILogger<ContributionManager> _logger;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;

        // Constructor to initialize dependencies
        public ContributionManager(
            IContributionRepository contributionDal,
            IMemberRepository memberDal,
            ITransactionManager transactionManager,
            IUnitOfWork unitOfWork,
            ILogger<ContributionManager> logger,
            IMapper mapper,
            ICacheService cacheService,
            IAccountManager accountManager,
            IFileManager fileManager,
            ITransactionRepository transactionDal)
        {
            _contributionDal = contributionDal;
            _memberDal = memberDal;
            _transactionManager = transactionManager;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
            _cacheService = cacheService;
            _accountManager = accountManager;
            _fileManager = fileManager;
            _transactionDal = transactionDal;
        }

        // Method to add a new contribution
        public async Task<IDataResult<ContributionDTO>> AddContributionAsync(ContributionRequest request)
        {
            _logger.LogInformation("AddContributionAsync called with request: {@Request}", request);

            // Retrieve member with accounts
            var member = await _memberDal.GetById(request.MemberId, includeProperties: a => a.Accounts);
            if (member == null)
            {
                _logger.LogWarning("Member not found with ID: {MemberId}", request.MemberId);
                return new ErrorDataResult<ContributionDTO>(null, ResponseCode_MemberAccountNotFound, ResponseMessage_MemberNotFound);
            }

            // Retrieve the appropriate pension account (Individual or Employer related)
            var pensionAccount = member.Accounts.FirstOrDefault(a => a.AccountType == request.AccountType);
            if (pensionAccount == null)
            {
                _logger.LogWarning("Pension account not found for member ID: {MemberId} and account type: {AccountType}", request.MemberId, request.AccountType);
                return new ErrorDataResult<ContributionDTO>(null, ResponseCode_PensionAccountNotFound, ResponseMessage_PensionAccountNotFound);
            }

            _logger.LogInformation("Starting transaction for contribution from member {MemberId}", request.MemberId);

            // Create contribution
            var contribution = new Contribution
            {
                MemberId = request.MemberId,
                Amount = request.Amount,
                PensionAccountNumber = pensionAccount.PensionAccountNumber,
                ContributionType = request.ContributionType,
            };

            // Add contribution to the database
            await _contributionDal.AddAsync(contribution);

            // Retrieve bank accounts to process payment based on the Contribution type (Individual induced or Employer Induced)
            var account = await _accountManager.RetrievePostingAccounts(request.AccountType, member);

            // Process transaction (simulated)
            var transactionResult = await _transactionManager.ProcessContributionTransactionAsync(
                memberId: request.MemberId,
                debitAccountBankCode: account.DebitAccountBank,
                debitAccountId: account.DebitAccount,
                creditAccountBankCode: account.DebitAccountBank,
                creditAccountId: account.DebitAccount,
                contributionId: contribution.ContributionId,
                remarks: $"{request.AccountType} Contribution",
                amount: request.Amount,
                accountType: request.AccountType,
                transactionType: TransactionType.Contribution);

            // Check if transaction was successful
            if (transactionResult.ResponseCode != ResponseCode_Success)
            {
                _logger.LogError("Transaction failed for contribution from member {MemberId} with response code: {ResponseCode}", request.MemberId, transactionResult.ResponseCode);
                await _unitOfWork.RollbackAsync();
                return new ErrorDataResult<ContributionDTO>(null, transactionResult.ResponseCode, transactionResult.ResponseDescription);
            }

            // Update pension account balance
            pensionAccount.CurrentBalance += request.Amount;
            pensionAccount.TotalContributions += 1;
            await _memberDal.Update(member);

            // Save changes and commit transaction
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            // Clear relevant caches
            _cacheService.Remove($"contributions_member_{request.MemberId}");
            _cacheService.Remove($"contributions_summary_{request.MemberId}");

            // Map contribution to DTO and return success result
            var contributionDto = _mapper.Map<ContributionDTO>(contribution);
            _logger.LogInformation("Contribution added successfully for member {MemberId}", request.MemberId);
            return new SuccessDataResult<ContributionDTO>(contributionDto);
        }

        // Method to get a contribution by its ID
        public async Task<IDataResult<ContributionDTO>> GetContributionByIdAsync(string id)
        {
            _logger.LogInformation("GetContributionByIdAsync called with ID: {Id}", id);

            string cacheKey = $"contribution_{id}";

            // Retrieve contribution from cache or database
            var contribution = await _cacheService.GetOrSetCacheAsync(
                cacheKey,
                async () => _mapper.Map<ContributionDTO>(await _contributionDal.Get(c => c.ContributionId == id)));

            // Return result based on whether contribution was found
            if (contribution == null)
            {
                _logger.LogWarning("Contribution not found with ID: {Id}", id);
                return new ErrorDataResult<ContributionDTO>(null, ResponseCode_ContributionNotFound, ResponseMessage_ContributionNotFound);
            }

            _logger.LogInformation("Contribution retrieved successfully with ID: {Id}", id);
            return new SuccessDataResult<ContributionDTO>(contribution);
        }

        // Method to generate statements for a member
        public async Task<IDataResult<StatementResult>> GenerateStatementsAsync(StatementRequest request)
        {
            _logger.LogInformation("GenerateStatementsAsync called with request: {@Request}", request);

            // Retrieve member and contributions
            var member = await _memberDal.Get(m => m.Id == request.MemberId);
            if (member == null)
            {
                _logger.LogWarning("Member not found with ID: {MemberId}", request.MemberId);
                return new ErrorDataResult<StatementResult>(null, ResponseCode_MemberNotFound, ResponseMessage_MemberNotFound);
            }

            var contributions = (await _contributionDal.GetAll(
                c => c.MemberId == request.MemberId &&
                     c.CreatedDate >= request.StartDate &&
                     c.CreatedDate <= request.EndDate))
                .OrderByDescending(c => c.CreatedDate)
                .ToList();

            if (!contributions.Any())
            {
                _logger.LogWarning("No contributions found for member ID: {MemberId} in the specified period", request.MemberId);
                return new ErrorDataResult<StatementResult>(null, ResponseCode_ContributionNotFoundSpecifiedPeriod, ResponseMessage_ContributionNotFoundSpecifiedPeriod);
            }

            StatementResult result;

            // Generate statement in the requested format
            if (request.Format == StatementFormat.PDF)
            {
                result = await _fileManager.GeneratePdfStatement(member, contributions, request.StartDate, request.EndDate);
            }
            else
            {
                result = await _fileManager.GenerateExcelStatement(member, contributions, request.StartDate, request.EndDate);
            }

            _logger.LogInformation("Statements generated successfully for member {MemberId}", request.MemberId);
            return new SuccessDataResult<StatementResult>(result);
        }

        // Method to get all contributions for a member
        public async Task<IDataResult<List<ContributionDTO>>> GetContributionsByMemberAsync(string memberId)
        {
            _logger.LogInformation("GetContributionsByMemberAsync called with member ID: {MemberId}", memberId);

            string cacheKey = $"contributions_member_{memberId}";

            // Retrieve contributions from cache or database
            var contributions = await _cacheService.GetOrSetCacheAsync(
                cacheKey,
                async () => _mapper.Map<List<ContributionDTO>>(
                    await _contributionDal.GetAll(c => c.MemberId == memberId)));

            _logger.LogInformation("Contributions retrieved successfully for member {MemberId}", memberId);
            return new SuccessDataResult<List<ContributionDTO>>(contributions);
        }

        // Method to get a summary of contributions for a member
        public async Task<IDataResult<ContributionSummaryDTO>> GetContributionSummaryAsync(string memberId)
        {
            _logger.LogInformation("GetContributionSummaryAsync called with member ID: {MemberId}", memberId);

            string cacheKey = $"contributions_summary_{memberId}";

            // Retrieve contribution summary from cache or database
            var summary = await _cacheService.GetOrSetCacheAsync(
                cacheKey,
                async () =>
                {
                    var contributions = await _contributionDal.GetAll(c => c.MemberId == memberId);

                    return new ContributionSummaryDTO
                    {
                        MemberId = memberId,
                        TotalContributions = contributions.Sum(c => c.Amount),
                        MonthlyContributions = contributions
                            .Where(c => c.ContributionType == ContributionType.Monthly)
                            .Sum(c => c.Amount),
                        VoluntaryContributions = contributions
                            .Where(c => c.ContributionType == ContributionType.Voluntary)
                            .Sum(c => c.Amount),
                        LastContributionDate = contributions.Any() ? contributions.Max(c => c.CreatedDate) : null,
                        ContributionCount = contributions.Count
                    };
                });

            _logger.LogInformation("Contribution summary retrieved successfully for member {MemberId}", memberId);
            return new SuccessDataResult<ContributionSummaryDTO>(summary);
        }

        public async Task<IDataResult<WithdrawalResult>> ProcessWithdrawalAsync(WithdrawalRequest request)
        {
            _logger.LogInformation("ProcessWithdrawalAsync called with request: {@Request}", request);

            // Retrieve member with accounts
            var member = await _memberDal.GetById(request.MemberId, includeProperties: a => a.Accounts);
            if (member == null)
            {
                _logger.LogWarning("Member not found with ID: {MemberId}", request.MemberId);
                return new ErrorDataResult<WithdrawalResult>(null, ResponseCode_MemberNotFound, ResponseMessage_MemberNotFound);
            }

            // Check eligibility
            if (!member.IsEligibleForBenefits)
            {
                _logger.LogWarning("Member not eligible for benefits with ID: {MemberId}", request.MemberId);
                return new ErrorDataResult<WithdrawalResult>(null, ResponseCode_MemberNotEligibleForBenefits, ResponseMessage_MemberNotEligibleForBenefits);
            }

            // Retrieve the appropriate pension account
            var pensionAccount = member.Accounts.FirstOrDefault(a => a.AccountType == request.accountType);
            if (pensionAccount == null)
            {
                _logger.LogWarning("Pension account not found for member ID: {MemberId} and account type: {AccountType}", request.MemberId, request.accountType);
                return new ErrorDataResult<WithdrawalResult>(null, ResponseCode_PensionAccountNotFound, ResponseMessage_PensionAccountNotFound);
            }

            // Check sufficient balance
            if (pensionAccount.CurrentBalance < request.Amount)
            {
                _logger.LogWarning("Insufficient funds for member ID: {MemberId} with requested amount: {Amount}", request.MemberId, request.Amount);
                return new ErrorDataResult<WithdrawalResult>(null, ResponseCode_InsufficientFunds, ResponseMessage_InsufficientFunds);
            }
            _logger.LogInformation("Starting withdrawal transaction for member {MemberId}", request.MemberId);

            // Retrieve bank accounts to process payment based on the Contribution type (Individual induced or Employer Induced)
            var account = await _accountManager.RetrievePostingAccounts(request.accountType, member);

            // Tuple deconstruction to swap the values of the account, since this is withdrawal
            (account.DebitAccount, account.CreditAccount) = (account.CreditAccount, account.DebitAccount);

            // Process withdrawal transaction
            var transactionResult = await _transactionManager.ProcessContributionTransactionAsync(
                memberId: request.MemberId,
                debitAccountBankCode: account.DebitAccountBank,
                debitAccountId: account.DebitAccount,
                creditAccountBankCode: account.DebitAccountBank,
                creditAccountId: account.DebitAccount,
                contributionId: string.Empty,
                remarks: $"Pension withdrawal from {request.accountType} account",
                amount: request.Amount,
                accountType: request.accountType,
                transactionType: TransactionType.Withdrawal);

            if (transactionResult.ResponseCode != ResponseCode_Success)
            {
                _logger.LogError("Transaction failed for withdrawal from member {MemberId} with response code: {ResponseCode}", request.MemberId, transactionResult.ResponseCode);
                await _unitOfWork.RollbackAsync();
                return new ErrorDataResult<WithdrawalResult>(null, transactionResult.ResponseCode, transactionResult.ResponseDescription);
            }

            // Update pension account balance
            pensionAccount.CurrentBalance -= request.Amount;
            await _memberDal.Update(member);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            // Clear relevant caches
            _cacheService.Remove($"contributions_member_{request.MemberId}");
            _cacheService.Remove($"contributions_summary_{request.MemberId}");

            var result = new WithdrawalResult
            {
                TransactionId = transactionResult.Data.TransactionId,
                MemberId = request.MemberId,
                Amount = request.Amount,
                NewBalance = pensionAccount.CurrentBalance,
                ProcessedDate = DateTime.UtcNow
            };

            _logger.LogInformation("Withdrawal processed successfully for member {MemberId}", request.MemberId);
            return new SuccessDataResult<WithdrawalResult>(result);
        }

        public async Task<IDataResult<EligibilityResultDTO>> CheckEligibilityAsync(string memberId)
        {
            _logger.LogInformation("CheckEligibilityAsync called with member ID: {MemberId}", memberId);

            var eligibility = new EligibilityResultDTO { MemberId = memberId };
            string cacheKey = $"eligibility_{memberId}";

            // Retrieve eligibility from cache or database
            var result = await _cacheService.GetOrSetCacheAsync<EligibilityResultDTO>(cacheKey, async () =>
            {
                var member = await _memberDal.Get(m => m.Id == memberId);
                if (member == null)
                {
                    _logger.LogWarning("Member not found with ID: {MemberId}", memberId);
                    return null;
                }
                eligibility.IsEligible = member.IsEligibleForBenefits;
                return eligibility;
            });

            // Return result based on whether eligibility was found
            if (result == null)
            {
                _logger.LogWarning("Eligibility not found for member ID: {MemberId}", memberId);
                return new ErrorDataResult<EligibilityResultDTO>(null, ResponseCode_MemberNotFound, ResponseMessage_MemberNotFound);
            }

            _logger.LogInformation("Eligibility retrieved successfully for member ID: {MemberId}", memberId);
            return new SuccessDataResult<EligibilityResultDTO>(result);
        }

        // Method to get unvalidated contributions since a specified cutoff date
        public async Task<List<Contribution>> GetUnvalidatedContributionsAsync(DateTime cutoffDate)
        {
            _logger.LogInformation("GetUnvalidatedContributionsAsync called with cutoff date: {CutoffDate}", cutoffDate);

            // Retrieve unvalidated contributions created on or after the cutoff date
            var contributions = await _contributionDal.GetAll(1, 1,
                c => !c.IsValidated && c.CreatedDate >= cutoffDate,
                includeProperties: mbox => mbox.Member);

            _logger.LogInformation("Unvalidated contributions retrieved successfully with cutoff date: {CutoffDate}", cutoffDate);
            return contributions.Items;
        }

        // Method to validate a contribution by its ID
        public async Task<IDataResult<string>> ValidateContributionAsync(string contributionId)
        {
            _logger.LogInformation("ValidateContributionAsync called with contribution ID: {ContributionId}", contributionId);

            // Retrieve the contribution by its ID
            var contribution = await _contributionDal.Get(c => c.ContributionId == contributionId);
            if (contribution == null)
            {
                _logger.LogWarning("Contribution not found with ID: {ContributionId}", contributionId);
                return new ErrorDataResult<string>(string.Empty, ResponseCode_ContributionNotFound, ResponseMessage_ContributionNotFound);
            }

            // Validate against transaction records
            var transaction = await _transactionDal.Get(t => t.ContributionId == contributionId);
            if (transaction == null || transaction.TransactionStatus != Persistence.Enums.TransactionStatus.Completed)
            {
                _logger.LogWarning("No valid transaction found for contribution ID: {ContributionId}", contributionId);
                return new ErrorDataResult<string>("No valid transaction found", "VAL01");
            }

            // Mark the contribution as validated
            contribution.IsValidated = true;
            await _contributionDal.Update(contribution);

            _logger.LogInformation("Contribution validated successfully with ID: {ContributionId}", contributionId);
            return new SuccessDataResult<string>(contributionId, "Validation successful");
        }

        // Method to get members who are approaching eligibility for benefits
        public async Task<List<Persistence.DBModels.Member>> GetMembersApproachingEligibilityAsync()
        {
            _logger.LogInformation("GetMembersApproachingEligibilityAsync called");

            // Calculate the minimum age for eligibility minus a 5-year window
            var minAge = ConfigSettings.ApplicationSetting.MinimumAgeForEligibility - 5;
            var minDate = DateTime.Now.AddYears(-minAge);

            // Retrieve members who are approaching eligibility
            var qualifiedMembers = await _memberDal.GetAll(1, 1,
                m => m.DateOfBirth <= minDate &&    // Born before cutoff (age >= minAge-5)
                     !m.IsEligibleForBenefits &&    // Not already eligible
                     m.Status == Status.Active,    // Only active members
                includeProperties: a => a.Accounts);    // Include related accounts

            _logger.LogInformation("Members approaching eligibility retrieved successfully");
            return qualifiedMembers.Items;
        }

        // Method to get accounts that qualify for interest calculation
        public async Task<List<Account>> GetAccountsForInterestCalculationAsync()
        {
            _logger.LogInformation("GetAccountsForInterestCalculationAsync called");

            // Retrieve accounts with a balance above the minimum required for interest calculation
            var minBalance = ConfigSettings.ApplicationSetting.MinimumBalanceForInterest;
            var qualifiedAAccounts = await _accountDal.GetAll(1, 1,
                a => a.CurrentBalance >= minBalance &&
                     a.Status == Status.Active);

            _logger.LogInformation("Accounts for interest calculation retrieved successfully");
            return qualifiedAAccounts.Items;
        }

        // Method to get the current interest rate for pension accounts
        public Task<decimal> GetCurrentInterestRateAsync()
        {
            _logger.LogInformation("GetCurrentInterestRateAsync called");

            // Retrieve the annual pension interest rate from configuration settings
            var rate = ConfigSettings.ApplicationSetting.PensionAnualRate;
            _logger.LogInformation("Current interest rate retrieved successfully");
            return Task.FromResult(rate / 12); // Return the monthly rate
        }
    }
}
