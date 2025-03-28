using Application.Interfaces.Business;
using Application.Interfaces.Database;
using Application.Interfaces.General;
using AutoMapper;
using Common.DTOs.Responses;
using Common.Models;
using Common.Services;
using Core.Results;
using Microsoft.Extensions.Logging;
using Persistence.DBModels;
using Persistence.Enums;
using static Common.Literals.StringLiterals;

namespace Application.Repositories
{
    public class BenefitEligibilityManager : IBenefitEligibilityManager
    {
        private readonly IBenefitEligibilityRepository _eligibilityDal;
        private readonly IEligibilityRuleRepository _eligibilityRuleDal;
        private readonly IMemberRepository _memberDal;
        private readonly IContributionRepository _contributionDal;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;
        private readonly ILogger<BenefitEligibilityManager> _logger;

        public BenefitEligibilityManager(
            IBenefitEligibilityRepository eligibilityDal,
            IMemberRepository memberDal,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<BenefitEligibilityManager> logger,
            IEligibilityRuleRepository eligibilityRuleDal,
            IContributionRepository contributionDal,
            ICacheService cacheService)
        {
            _eligibilityDal = eligibilityDal;
            _memberDal = memberDal;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _eligibilityRuleDal = eligibilityRuleDal;
            _contributionDal = contributionDal;
            _cacheService = cacheService;
        }

        /// <summary>
        /// Recalculates the eligibility of a member for benefits.
        /// </summary>
        /// <param name="memberId">The ID of the member whose eligibility is to be recalculated.</param>
        /// <returns>A task representing the asynchronous operation, returning the eligibility result.</returns>
        public async Task<IDataResult<EligibilityResultDTO>> RecalculateEligibilityAsync(string memberId)
        {
            _logger.LogInformation("Starting eligibility recalculation for member {MemberId}", memberId);

            try
            {
                _logger.LogInformation("Fetching member data for member {MemberId}", memberId);
                var member = await _memberDal.GetById(memberId, includeProperties: m => m.Accounts);

                // Validate if member exists before proceeding
                if (member == null)
                {
                    _logger.LogWarning("Member {MemberId} not found", memberId);
                    return new ErrorDataResult<EligibilityResultDTO>(null, ResponseCode_MemberAccountNotFound, ResponseMessage_MemberNotFound);
                }

                _logger.LogInformation("Fetching eligibility rules");
                var rules = await _eligibilityRuleDal.GetAll(); // Retrieve all configured eligibility rules

                _logger.LogInformation("Verifying eligibility rules for member {MemberId}", memberId);
                var verification = await VerifyAllEligibilityRules(member, rules); // Verify if memeber satisifes the rules or not

                bool eligibilityChanged = member.IsEligibleForBenefits != verification.IsEligible; // Update the eligibilty status of member, if verification returned otherwise
                if (eligibilityChanged)
                {
                    _logger.LogInformation("Eligibility status changed for member {MemberId}. Updating status.", memberId);
                    member.IsEligibleForBenefits = verification.IsEligible;
                    member.ModifiedDate = DateTime.UtcNow;
                    await _memberDal.Update(member);
                }

                _logger.LogInformation("Fetching benefit eligibility record for member {MemberId}", memberId);
                var benefitEligibility = (await _eligibilityDal.GetAll(x => x.BenefitType == BenefitType.Retirement))?.FirstOrDefault(); // Create record for benefit eligibilty for reference
                if (benefitEligibility == null)
                {
                    _logger.LogInformation("Creating new benefit eligibility record for member {MemberId}", memberId);
                    benefitEligibility = new BenefitEligibility
                    {
                        BenefitEligibilityId = Ulid.NewUlid().ToString(),
                        BenefitType = BenefitType.Retirement,
                        MemberId = memberId,
                        Status = Status.Active
                    };
                    await _eligibilityDal.AddAsync(benefitEligibility);
                }

                // Update the remaining eligibility details
                benefitEligibility.IsEligible = verification.IsEligible;
                benefitEligibility.EligibilityDate = DateTime.UtcNow;
                benefitEligibility.EligibilityReason = verification.IsEligible
                    ? "Met all eligibility requirements"
                    : $"Failed requirements: {string.Join(", ", verification.FailedRequirements)}";

                _logger.LogInformation("Updating benefit eligibility record for member {MemberId}", memberId);
                await _eligibilityDal.Update(benefitEligibility);

                _logger.LogInformation("Committing transaction for member {MemberId}", memberId);
                await _unitOfWork.CommitAsync();

                // Clear cache, so update applies immmidiately incase data is queried
                _logger.LogInformation("Clearing caches for member {MemberId}", memberId);
                _cacheService.Remove($"eligibility_{memberId}");
                _cacheService.Remove($"benefits_{memberId}");

                _logger.LogInformation("Eligibility recalculation completed for member {MemberId}. Eligible: {IsEligible}", memberId, verification.IsEligible);
                return new SuccessDataResult<EligibilityResultDTO>(verification);
            }
            catch (Exception ex)
            {
                // Roll back changes, so `ATOMICITY` is guaranteed
                await _unitOfWork.Rollback();
                _logger.LogError(ex, "Error recalculating eligibility for member {MemberId}", memberId);
                return new ErrorDataResult<EligibilityResultDTO>(null, "09", "System error during eligibility recalculation");
            }
        }

        private async Task<EligibilityResultDTO> VerifyAllEligibilityRules(Member member, List<EligibilityRule> rules)
        {
            // Validate user against system eligibilty rules
            _logger.LogInformation("Starting eligibility verification for member {MemberId}", member.Id);
            var result = new EligibilityResultDTO
            {
                MemberId = member.Id,
                EvaluationDate = DateTime.UtcNow,
                BenefitType = BenefitType.Retirement,
            };

            var failedRules = new List<string>();
            var passedRules = new List<string>();

            _logger.LogInformation("Fetching member contributions for member {MemberId}", member.Id);
            var memberContributions = await _contributionDal.GetAll();

            foreach (var rule in rules.Where(r => r.IsActive))
            {
                _logger.LogInformation("Evaluating rule {RuleName} for member {MemberId}", rule.RuleName, member.Id);
                bool rulePassed = rule.RuleName switch
                {
                    "MinimumAge" => VerifyMinimumAge(member.DateOfBirth, rule.ThresholdValue),
                    "MinimumContributions" => VerifyMinimumContributions(memberContributions, rule.ThresholdValue),
                    "AccountActive" => VerifyActiveAccount(member.Accounts),
                    _ => true
                };

                if (rulePassed)
                {
                    _logger.LogInformation("Rule {RuleName} passed for member {MemberId}", rule.RuleName, member.Id);
                    passedRules.Add(rule.Description);
                }
                else
                {
                    _logger.LogWarning("Rule {RuleName} failed for member {MemberId}", rule.RuleName, member.Id);
                    failedRules.Add(rule.Description);
                }
            }

            result.IsEligible = failedRules.Count == 0;
            result.FailedRequirements = failedRules;
            result.PassedRequirements = passedRules;
            result.EligibilityDate = DateTime.UtcNow;

            _logger.LogInformation("Eligibility verification completed for member {MemberId}. Eligible: {IsEligible}", member.Id, result.IsEligible);
            return result;
        }

        public bool VerifyMinimumAge(DateTime dob, decimal? thresholdAge)
        {
            var age = DateTime.Now.Year - dob.Year;
            if (dob.Date > DateTime.Now.AddYears(-age)) age--;
            return age >= thresholdAge;
        }

        public bool VerifyMinimumContributions(ICollection<Contribution> contributions, decimal? threshold)
        {
            return contributions.Count(c =>
                c.ContributionType == ContributionType.Monthly &&
                c.IsValidated) >= threshold;
        }

        public bool VerifyActiveAccount(ICollection<Account> accounts)
        {
            return accounts.Any(a => a.Status == Status.Active && !a.IsRestricted && !a.IsClosed);
        }
    }
}
