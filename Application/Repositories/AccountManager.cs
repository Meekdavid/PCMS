using Application.Interfaces.Business;
using Application.Interfaces.Database;
using Common.ConfigurationSettings;
using Common.Models;
using Persistence.DBModels;
using Persistence.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using Microsoft.Extensions.Logging;
using Common.DTOs.Requests;
using Common.DTOs.Responses;
using Common.Pagination;
using Common.Services;
using Core.Results;
using AutoMapper;
using static Common.Literals.StringLiterals;
using Application.Interfaces.General;

namespace Application.Repositories
{
    public class AccountManager : IAccountManager
    {
        private readonly IEmployerRepository _employerDal;
        private readonly IAccountRepository _accountDal;
        private readonly IMemberRepository _memberDal;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AccountManager> _logger;
        private readonly ICacheService _cacheService;
        private readonly IMapper _mapper;

        public AccountManager(
            IEmployerRepository employerDal,
            ILogger<AccountManager> logger,
            ICacheService cacheService,
            IAccountRepository accountRepository,
            IMapper mapper,
            IUnitOfWork unitOfWork,
            IMemberRepository memberDal)
        {
            _employerDal = employerDal;
            _logger = logger;
            _cacheService = cacheService;
            _accountDal = accountRepository;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _memberDal = memberDal;
        }

        public async Task<PostingAccounts> RetrievePostingAccounts(AccountType accountType, Member member)
        {
            _logger.LogInformation("Retrieving posting accounts for account type: {AccountType} and member: {MemberId}", accountType, member.Id);

            return accountType switch
            {
                AccountType.EmployerSponsoredPension => await GetEmployerSponsoredPensionAccounts(member),
                AccountType.IndividualContribution => await GetIndividualContributionAccounts(member),
                _ => throw new ArgumentOutOfRangeException(nameof(accountType), accountType, null)
            };
        }

        private async Task<PostingAccounts> GetEmployerSponsoredPensionAccounts(Member member)
        {
            _logger.LogInformation("Retrieving employer-sponsored pension accounts for member: {MemberId}", member.Id);

            if (member.EmployerId == null)
            {
                _logger.LogError("EmployerId is null for member: {MemberId}", member.Id);
                throw new ArgumentNullException(nameof(member.EmployerId), "EmployerId cannot be null");
            }

            var employer = await _employerDal.GetByIdAsync(member.EmployerId);

            if (employer == null)
            {
                _logger.LogError("Employer not found for EmployerId: {EmployerId}", member.EmployerId);
                throw new InvalidOperationException($"Employer not found for EmployerId: {member.EmployerId}");
            }

            _logger.LogInformation("Successfully retrieved employer-sponsored pension accounts for member: {MemberId}", member.Id);

            return new PostingAccounts
            {
                DebitAccount = employer.BankAccountNumber,
                DebitAccountBank = employer.BankName,
                CreditAccount = ConfigSettings.ApplicationSetting.NLPCAccountId,
                CreditAccountBank = ConfigSettings.ApplicationSetting.NLPCBank
            };
        }

        private async Task<PostingAccounts> GetIndividualContributionAccounts(Member member)
        {
            _logger.LogInformation("Retrieving individual contribution accounts for member: {MemberId}", member.Id);

            return await Task.FromResult(new PostingAccounts
            {
                DebitAccount = member.BankAccountNumber,
                DebitAccountBank = member.BankName,
                CreditAccount = ConfigSettings.ApplicationSetting.NLPCAccountId,
                CreditAccountBank = ConfigSettings.ApplicationSetting.NLPCBank
            });
        }

        public async Task<IDataResult<PaginatedList<AccountDTO>>> RetrieveAllAccountsAsync(int pageIndex, int pageSize)
        {
            _logger.LogInformation($"Received request to retrieve all accounts with pageSize {pageSize} and pageIndex {pageIndex}");

            string cacheKeyword = $"RetrieveAllAccounts_{pageIndex}_{pageSize}";

            return await _cacheService.GetOrSetCacheAsync(
                cacheKeyword,
                async () =>
                {
                    var accounts = await _accountDal.GetAll(pageIndex, pageSize, null, includeProperties: a => a.Member);
                    var mappedAccounts = _mapper.Map<PaginatedList<AccountDTO>>(accounts);
                    return new SuccessDataResult<PaginatedList<AccountDTO>>(mappedAccounts);
                }
            );
        }

        public async Task<IDataResult<AccountDTO>> RetrieveAccountByIdAsync(string accountId)
        {
            _logger.LogInformation($"Received request to retrieve account with ID: {accountId}");

            string cacheKeyword = $"RetrieveAccountById_{accountId}";

            var account = await _cacheService.GetOrSetCacheAsync<AccountDTO>(
                cacheKeyword,
                async () =>
                {
                    var account = await _accountDal.Get(a => a.AccountId == accountId);
                    return _mapper.Map<AccountDTO>(account);
                }
            );

            if (account == null)
            {
                _logger.LogWarning($"Account with ID {accountId} not found.");
                return new ErrorDataResult<AccountDTO>(null, ResponseCode_AccountNotFound, ResponseMessage_AccountNotFound);
            }

            return new SuccessDataResult<AccountDTO>(account);
        }

        public async Task<IDataResult<AccountDTO>> AddNewAccountAsync(NewAccountRequest request)
        {
            _logger.LogInformation("Received request to add new account.");

            var member = await _memberDal.Get(m => m.Id == request.MemberId);
            if (member == null)
            {
                _logger.LogWarning("Member not found.");
                return new ErrorDataResult<AccountDTO>(null, ResponseCode_MemberNotFound, ResponseMessage_MemberNotFound);
            }

            var random = new Random();
            string pensionAccountNumber = random.Next(1000000000, 2147483647).ToString();

            var newAccount = new Account
            {
                MemberId = request.MemberId,
                EmployerId = member.EmployerId ?? "N/A",
                PensionAccountNumber = pensionAccountNumber,
                AccountType = request.AccountType,
                TotalContributions = 0,
                CurrentBalance = 0,
                IsRestricted = false,
                IsClosed = false
            };

            await _accountDal.AddAsync(newAccount);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            var accountDto = _mapper.Map<AccountDTO>(newAccount);
            return new SuccessDataResult<AccountDTO>(accountDto);
        }

        public async Task<IDataResult<PaginatedList<AccountDTO>>> GetAccountsForSpecificMemberAsync(string memberId)
        {
            _logger.LogInformation($"Received request to retrieve accounts for member {memberId} with pageSize 1 and pageIndex 1");

            if (string.IsNullOrWhiteSpace(memberId))
            {
                _logger.LogWarning("Member ID cannot be empty.");
                return new ErrorDataResult<PaginatedList<AccountDTO>>(null, "Member ID cannot be empty", "14");
            }

            string cacheKeyword = $"GetAccountsForMember_{memberId}";

            return await _cacheService.GetOrSetCacheAsync(
                cacheKeyword,
                async () =>
                {
                    var accounts = await _accountDal.GetAll(1, 1, a => a.MemberId == memberId);
                    var mappedAccounts = _mapper.Map<PaginatedList<AccountDTO>>(accounts);
                    return new SuccessDataResult<PaginatedList<AccountDTO>>(mappedAccounts);
                }
            );
        }
    }
}
