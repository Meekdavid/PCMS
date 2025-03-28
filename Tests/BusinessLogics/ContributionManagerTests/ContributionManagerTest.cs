using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using FakeItEasy;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Application.Interfaces.Business;
using Application.Interfaces.Database;
using Application.Interfaces.General;
using Application.Repositories;
using Common.DTOs.Requests;
using Common.DTOs.Responses;
using Common.Pagination;
using Common.Services;
using Core.Results;
using Persistence.DBModels;
using Persistence.Enums;
using Common.Models;
using static Common.Literals.StringLiterals;

namespace Tests.BusinessLogics.ContributionManagerTests
{
 

    public class ContributionManagerTest
    {
        private readonly IContributionRepository _contributionDal;
        private readonly IMemberRepository _memberDal;
        private readonly ITransactionManager _transactionManager;
        private readonly IAccountManager _accountManager;
        private readonly IFileManager _fileManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITransactionRepository _transactionDal;
        private readonly IAccountRepository _accountDal;
        private readonly ILogger<ContributionManager> _logger;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;
        private readonly ContributionManager _manager;

        public ContributionManagerTest()
        {
            _contributionDal = A.Fake<IContributionRepository>();
            _memberDal = A.Fake<IMemberRepository>();
            _transactionManager = A.Fake<ITransactionManager>();
            _accountDal = A.Fake<IAccountRepository> ();
            _accountManager = A.Fake<IAccountManager>();
            _fileManager = A.Fake<IFileManager>();
            _unitOfWork = A.Fake<IUnitOfWork>();
            _transactionDal = A.Fake<ITransactionRepository>();
            _logger = A.Fake<ILogger<ContributionManager>>();
            _mapper = A.Fake<IMapper>();
            _cacheService = A.Fake<ICacheService>();

            _manager = new ContributionManager(
                _contributionDal,
                _memberDal,
                _transactionManager,
                _unitOfWork,
                _logger,
                _mapper,
                _cacheService,
                _accountManager,
                _fileManager,
                _transactionDal,
                _accountDal);
        }

        [Fact]
        public async Task AddContributionAsync_ValidRequest_CreatesContribution()
        {
            // Arrange
            var request = new ContributionRequest
            {
                MemberId = "mem-123",
                Amount = 1000,
                AccountType = AccountType.IndividualContribution,
                ContributionType = ContributionType.Monthly
            };

            var member = new Member
            {
                Id = "mem-123",
                Accounts = new List<Account>
            {
                new Account
                {
                    AccountType = AccountType.IndividualContribution,
                    PensionAccountNumber = "IND-ACC-123"
                }
            }
            };

            var accountInfo = new PostingAccounts
            {
                DebitAccount = "BANK-123",
                DebitAccountBank = "BANK-CODE",
                CreditAccount = "PENSION-ACC",
                CreditAccountBank = "PENSION-BANK"
            };

            var transactionResult = new SuccessDataResult<TransactionDTO>(new TransactionDTO());

            A.CallTo(() => _memberDal.GetById(request.MemberId, A<Expression<Func<Member, object>>>._))
                .Returns(Task.FromResult(member));

            A.CallTo(() => _accountManager.RetrievePostingAccounts(request.AccountType, member))
                .Returns(Task.FromResult(accountInfo));
            A.CallTo(() => _transactionManager.ProcessContributionTransactionAsync(
                A<string>._, // memberId
                A<string>._, // debitAccountBankCode
                A<string>._, // debitAccountId
                A<string>._, // creditAccountBankCode
                A<string>._, // creditAccountId
                A<string>._, // contributionId
                A<decimal>._, // amount
                A<string>._, // remarks                
                A<AccountType>._, // accountType
                A<TransactionType>._)) // transactionType
            .Returns(Task.FromResult<IDataResult<TransactionDTO>>(transactionResult));

            A.CallTo(() => _mapper.Map<ContributionDTO>(A<Contribution>._))
                .Returns(new ContributionDTO());

            // Act
            var result = await _manager.AddContributionAsync(request);

            // Assert
            Assert.Equal(ResponseCode_Success, result.ResponseCode);
            A.CallTo(() => _contributionDal.AddAsync(A<Contribution>._)).MustHaveHappened();
            A.CallTo(() => _unitOfWork.CommitAsync()).MustHaveHappened();
            A.CallTo(() => _cacheService.Remove($"contributions_member_{request.MemberId}")).MustHaveHappened();
        }

        [Fact]
        public async Task GetContributionByIdAsync_ValidId_ReturnsContribution()
        {
            // Arrange
            var contributionId = "cont-123";
            var contribution = new Contribution { ContributionId = contributionId };
            var contributionDto = new ContributionDTO { ContributionId = contributionId };

            A.CallTo(() => _contributionDal.Get(A<Expression<Func<Contribution, bool>>>._))
                .Returns(contribution);

            A.CallTo(() => _mapper.Map<ContributionDTO>(contribution))
                .Returns(contributionDto);

            A.CallTo(() => _cacheService.GetOrSetCacheAsync(
                    A<string>._,
                    A<Func<Task<ContributionDTO>>>._))
                .ReturnsLazily(async call => await call.GetArgument<Func<Task<ContributionDTO>>>(1)());

            // Act
            var result = await _manager.GetContributionByIdAsync(contributionId);

            // Assert
            Assert.Equal(ResponseCode_Success, result.ResponseCode);
            Assert.Equal(contributionId, result.Data.ContributionId);
        }

        [Fact]
        public async Task GenerateStatementsAsync_PDFFormat_GeneratesPdf()
        {
            // Arrange
            var request = new StatementRequest
            {
                MemberId = "mem-123",
                Format = StatementFormat.PDF,
                StartDate = DateTime.Now.AddMonths(-1),
                EndDate = DateTime.Now
            };

            var member = new Member { Id = "mem-123" };
            var contributions = new List<Contribution>
        {
            new Contribution { MemberId = "mem-123" }
        };

            var statementResult = new StatementResult();

            A.CallTo(() => _memberDal.Get(A<Expression<Func<Member, bool>>>._))
                .Returns(member);

            A.CallTo(() => _contributionDal.GetAll(
                A<Expression<Func<Contribution, bool>>>._))
                .Returns(Task.FromResult(contributions));

            A.CallTo(() => _fileManager.GeneratePdfStatement(
                member, contributions, request.StartDate, request.EndDate))
                .Returns(Task.FromResult(statementResult));

            // Act
            var result = await _manager.GenerateStatementsAsync(request);

            // Assert
            Assert.Equal(ResponseCode_Success, result.ResponseCode);
            A.CallTo(() => _fileManager.GeneratePdfStatement(
                member, contributions, request.StartDate, request.EndDate)).MustHaveHappened();
        }

        [Fact]
        public async Task ProcessWithdrawalAsync_InsufficientFunds_ReturnsError()
        {
            // Arrange
            var request = new WithdrawalRequest
            {
                MemberId = "mem-123",
                Amount = 10000,
                accountType = AccountType.IndividualContribution
            };

            var member = new Member
            {
                Id = "mem-123",
                IsEligibleForBenefits = true,
                Accounts = new List<Account>
            {
                new Account
                {
                    AccountType = AccountType.IndividualContribution,
                    CurrentBalance = 5000
                }
            }
            };

            A.CallTo(() => _memberDal.GetById(request.MemberId, A<Expression<Func<Member, object>>>._))
                .Returns(Task.FromResult(member));

            // Act
            var result = await _manager.ProcessWithdrawalAsync(request);

            // Assert
            Assert.Equal(ResponseCode_InsufficientFunds, result.ResponseCode);
        }

        [Fact]
        public async Task GetUnvalidatedContributionsAsync_ReturnsUnvalidatedContributions()
        {
            // Arrange
            var cutoffDate = DateTime.Now.AddDays(-30);
            var contributions = new List<Contribution>
        {
            new Contribution { IsValidated = false, CreatedDate = DateTime.Now }
        };

            var paginated = new PaginatedList<Contribution>(contributions, 1, 1, 10);

            A.CallTo(() => _contributionDal.GetAll(
                A<int>._,
                A<int>._,
                A<Expression<Func<Contribution, bool>>>._,
                A<Expression<Func<Contribution, object>>>._))
                .Returns(paginated);

            // Act
            var result = await _manager.GetUnvalidatedContributionsAsync(cutoffDate);

            // Assert
            Assert.Single(result);
            Assert.False(result[0].IsValidated);
        }

        [Fact]
        public async Task ValidateContributionAsync_ValidContribution_UpdatesValidation()
        {
            // Arrange
            var contributionId = "cont-123";
            var contribution = new Contribution { ContributionId = contributionId };
            var transaction = new Transaction { TransactionStatus = TransactionStatus.Completed };

            A.CallTo(() => _contributionDal.Get(A<Expression<Func<Contribution, bool>>>._))
                .Returns(contribution);

            A.CallTo(() => _transactionDal.Get(A<Expression<Func<Transaction, bool>>>._))
                .Returns(transaction);

            // Act
            var result = await _manager.ValidateContributionAsync(contributionId);

            // Assert
            Assert.Equal(ResponseCode_Success, result.ResponseCode);
            A.CallTo(() => _contributionDal.Update(contribution)).MustHaveHappened();
        }

        [Fact]
        public async Task GetCurrentInterestRateAsync_ReturnsMonthlyRate()
        {
            // Arrange
            var annualRate = 12m; // 12% annual
            var expectedMonthlyRate = 1m; // 1% monthly

            // Act
            var result = await _manager.GetCurrentInterestRateAsync();

            // Assert
            Assert.Equal(expectedMonthlyRate, result);
        }
    }
}
