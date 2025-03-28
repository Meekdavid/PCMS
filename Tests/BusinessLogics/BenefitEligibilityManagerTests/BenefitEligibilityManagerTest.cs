using Application.Interfaces.Database;
using Application.Interfaces.General;
using Application.Repositories;
using AutoMapper;
using Common.Services;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Persistence.DBModels;
using Persistence.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static Common.Literals.StringLiterals;

namespace Tests.BusinessLogics.BenefitEligibilityManagerTests
{
    public class BenefitEligibilityManagerTest
    {
        private readonly IBenefitEligibilityRepository _eligibilityDal;
        private readonly IEligibilityRuleRepository _eligibilityRuleDal;
        private readonly IMemberRepository _memberDal;
        private readonly IContributionRepository _contributionDal;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;
        private readonly ILogger<BenefitEligibilityManager> _logger;
        private readonly BenefitEligibilityManager _manager;

        public BenefitEligibilityManagerTest()
        {
            _eligibilityDal = A.Fake<IBenefitEligibilityRepository>();
            _eligibilityRuleDal = A.Fake<IEligibilityRuleRepository>();
            _memberDal = A.Fake<IMemberRepository>();
            _contributionDal = A.Fake<IContributionRepository>();
            _unitOfWork = A.Fake<IUnitOfWork>();
            _mapper = A.Fake<IMapper>();
            _cacheService = A.Fake<ICacheService>();
            _logger = A.Fake<ILogger<BenefitEligibilityManager>>();

            _manager = new BenefitEligibilityManager(
                _eligibilityDal,
                _memberDal,
                _unitOfWork,
                _mapper,
                _logger,
                _eligibilityRuleDal,
                _contributionDal,
                _cacheService);
        }

        [Fact]
        public async Task RecalculateEligibilityAsync_MemberNotFound_ReturnsError()
        {
            // Arrange
            var memberId = "non-existent";
            A.CallTo(() => _memberDal.GetById(memberId, A<Expression<Func<Member, object>>>._))
                .Returns((Member)null);

            // Act
            var result = await _manager.RecalculateEligibilityAsync(memberId);

            // Assert
            Assert.Equal(ResponseCode_MemberAccountNotFound, result.ResponseCode);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task RecalculateEligibilityAsync_AllRulesPass_ReturnsEligible()
        {
            // Arrange
            var memberId = "mem-123";
            var member = new Member
            {
                Id = memberId,
                DateOfBirth = DateTime.Now.AddYears(-25),
                Accounts = new List<Account> { new Account { Status = Status.Active } }
            };

            var rules = new List<EligibilityRule>
    {
        new EligibilityRule { RuleName = "MinimumAge", ThresholdValue = 18, IsActive = true },
        new EligibilityRule { RuleName = "MinimumContributions", ThresholdValue = 12, IsActive = true },
        new EligibilityRule { RuleName = "AccountActive", IsActive = true }
    };

            var contributions = new List<Contribution>
    {
        new Contribution { ContributionType = ContributionType.Monthly, IsValidated = true },
        // Add enough to meet threshold
    };

            A.CallTo(() => _memberDal.GetById(memberId, A<Expression<Func<Member, object>>>._))
                .Returns(member);

            A.CallTo(() => _eligibilityRuleDal.GetAll(x => x.Status == Status.Active))
                .Returns(Task.FromResult(rules));

            // Corrected mock setups:
            A.CallTo(() => _contributionDal.GetAll(A<Expression<Func<Contribution, bool>>>._))
                .Returns(Task.FromResult(contributions));

            A.CallTo(() => _eligibilityDal.GetAll(A<Expression<Func<BenefitEligibility, bool>>>._))
                .Returns(Task.FromResult(new List<BenefitEligibility>()));

            // Act
            var result = await _manager.RecalculateEligibilityAsync(memberId);

            // Assert
            Assert.True(result.Data.IsEligible);
        }

        [Fact]
        public async Task RecalculateEligibilityAsync_SomeRulesFail_ReturnsIneligible()
        {
            // Arrange
            var memberId = "mem-123";
            var member = new Member
            {
                Id = memberId,
                DateOfBirth = DateTime.Now.AddYears(-17), // Underage
                Accounts = new List<Account> { new Account { Status = Status.Active } }
            };

            var rules = new List<EligibilityRule>
        {
            new EligibilityRule { RuleName = "MinimumAge", ThresholdValue = 18, Description = "Must be 18+", IsActive = true },
            new EligibilityRule { RuleName = "MinimumContributions", ThresholdValue = 12, IsActive = true }
        };

            A.CallTo(() => _memberDal.GetById(memberId, A<Expression<Func<Member, object>>>._))
                .Returns(member);

            A.CallTo(() => _eligibilityRuleDal.GetAll(x => x.IsActive))
                .Returns(rules);

            // Act
            var result = await _manager.RecalculateEligibilityAsync(memberId);

            // Assert
            Assert.False(result.Data.IsEligible);
            Assert.Contains("Must be 18+", result.Data.FailedRequirements);
        }

        [Fact]
        public async Task RecalculateEligibilityAsync_ExceptionOccurs_RollsBackTransaction()
        {
            // Arrange
            var memberId = "mem-123";
            A.CallTo(() => _memberDal.GetById(memberId, A<Expression<Func<Member, object>>>._))
                .Throws(new Exception("Simulated error"));

            // Act
            var result = await _manager.RecalculateEligibilityAsync(memberId);

            // Assert
            Assert.Equal("09", result.ResponseCode);
            A.CallTo(() => _unitOfWork.Rollback()).MustHaveHappened();
        }

        [Fact]
        public async Task RecalculateEligibilityAsync_ExistingEligibilityRecord_UpdatesInsteadOfCreating()
        {
            // Arrange
            var memberId = "mem-123";
            var member = new Member { Id = memberId, DateOfBirth = DateTime.Now.AddYears(-30) };
            var existingEligibility = new BenefitEligibility { BenefitEligibilityId = "existing-id" };

            // Mock member retrieval (assuming GetById returns Task<Member>)
            A.CallTo(() => _memberDal.GetById(memberId, A<Expression<Func<Member, object>>>._))
                .Returns(Task.FromResult(member));

            // Correct way to mock GetAll when it returns Task<List<T>>
            A.CallTo(() => _eligibilityDal.GetAll(A<Expression<Func<BenefitEligibility, bool>>>._))
                .Returns(Task.FromResult(new List<BenefitEligibility> { existingEligibility }));

            // Act
            await _manager.RecalculateEligibilityAsync(memberId);

            // Assert
            A.CallTo(() => _eligibilityDal.AddAsync(A<BenefitEligibility>._)).MustNotHaveHappened();
            A.CallTo(() => _eligibilityDal.Update(existingEligibility)).MustHaveHappened();
        }

        // ===== Rule Verification Tests =====
        [Fact]
        public void VerifyMinimumAge_WhenAgeAboveThreshold_ReturnsTrue()
        {
            // Arrange
            var dob = DateTime.Now.AddYears(-20);
            decimal threshold = 18;

            // Act
            var result = _manager.TestVerifyMinimumAge(dob, threshold);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void VerifyMinimumContributions_WhenContributionsMeetThreshold_ReturnsTrue()
        {
            // Arrange
            var contributions = new List<Contribution>
        {
            new Contribution { ContributionType = ContributionType.Monthly, IsValidated = true },
            new Contribution { ContributionType = ContributionType.Monthly, IsValidated = true }
        };
            decimal threshold = 2;

            // Act
            var result = _manager.TestVerifyMinimumContributions(contributions, threshold);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void VerifyActiveAccount_WhenHasActiveAccount_ReturnsTrue()
        {
            // Arrange
            var accounts = new List<Account>
        {
            new Account { Status = Status.Active, IsRestricted = false, IsClosed = false }
        };

            // Act
            var result = _manager.TestVerifyActiveAccount(accounts);

            // Assert
            Assert.True(result);
        }
    }

    // Test helper extension to access private methods
    public static class BenefitEligibilityManagerTestExtensions
    {
        public static bool TestVerifyMinimumAge(this BenefitEligibilityManager manager, DateTime dob, decimal? thresholdAge)
        {
            return manager.VerifyMinimumAge(dob, thresholdAge);
        }

        public static bool TestVerifyMinimumContributions(this BenefitEligibilityManager manager, ICollection<Contribution> contributions, decimal? threshold)
        {
            return manager.VerifyMinimumContributions(contributions, threshold);
        }

        public static bool TestVerifyActiveAccount(this BenefitEligibilityManager manager, ICollection<Account> accounts)
        {
            return manager.VerifyActiveAccount(accounts);
        }
    }
}
