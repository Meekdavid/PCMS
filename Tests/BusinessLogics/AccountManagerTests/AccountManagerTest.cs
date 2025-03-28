using Application.Interfaces.Database;
using Application.Interfaces.General;
using Application.Repositories;
using AutoMapper;
using Common.ConfigurationSettings;
using Common.DTOs.Requests;
using Common.DTOs.Responses;
using Common.Models;
using Common.Pagination;
using Common.Services;
using FakeItEasy;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Persistence.DBModels;
using Persistence.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Tests.Shared.TestInterfaces;
using Xunit;
using static Common.Literals.StringLiterals;

namespace Tests.BusinessLogics.AccountManagerTests
{
    public class AccountManagerTest
    {
        private readonly IEmployerRepository _employerRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly IMemberRepository _memberRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AccountManager> _logger;
        private readonly Common.Services.ICacheService _cacheService;
        private readonly IMapper _mapper;
        private readonly AccountManager _accountManager;

        public AccountManagerTest()
        {
            _employerRepository = A.Fake<IEmployerRepository>();
            _accountRepository = A.Fake<IAccountRepository>();
            _memberRepository = A.Fake<IMemberRepository>();
            _unitOfWork = A.Fake<IUnitOfWork>();
            _logger = A.Fake<ILogger<AccountManager>>();
            _cacheService = A.Fake<Common.Services.ICacheService>();
            _mapper = A.Fake<IMapper>();

            _accountManager = new AccountManager(
                _employerRepository,
                _logger,
                _cacheService,
                _accountRepository,
                _mapper,
                _unitOfWork,
                _memberRepository);
        }

        [Fact]
        public async Task RetrievePostingAccounts_EmployerSponsored_ReturnsCorrectAccounts()
        {
            // Arrange
            var member = new Member
            {
                Id = "mem-123",
                EmployerId = "emp-456",
                BankAccountNumber = "1234567890",
                BankName = "Test Bank"
            };

            var employer = new Employer
            {
                EmployerId = "emp-456",
                BankAccountNumber = "EMPLOYER-ACC",
                BankName = "Employer Bank"
            };

            A.CallTo(() => _employerRepository.GetByIdAsync("emp-456"))
                .Returns(employer);

            // Act
            var result = await _accountManager.RetrievePostingAccounts(AccountType.EmployerSponsoredPension, member);

            // Assert
            Assert.Equal("EMPLOYER-ACC", result.DebitAccount);
            Assert.Equal("Employer Bank", result.DebitAccountBank);
            Assert.Equal(ConfigSettings.ApplicationSetting.NLPCAccountId, result.CreditAccount);
            Assert.Equal(ConfigSettings.ApplicationSetting.NLPCBank, result.CreditAccountBank);
        }

        [Fact]
        public async Task RetrievePostingAccounts_IndividualContribution_ReturnsCorrectAccounts()
        {
            // Arrange
            var member = new Member
            {
                Id = "mem-123",
                BankAccountNumber = "INDIVIDUAL-ACC",
                BankName = "Member Bank"
            };

            // Act
            var result = await _accountManager.RetrievePostingAccounts(AccountType.IndividualContribution, member);

            // Assert
            Assert.Equal("INDIVIDUAL-ACC", result.DebitAccount);
            Assert.Equal("Member Bank", result.DebitAccountBank);
            Assert.Equal(ConfigSettings.ApplicationSetting.NLPCAccountId, result.CreditAccount);
            Assert.Equal(ConfigSettings.ApplicationSetting.NLPCBank, result.CreditAccountBank);
        }

        [Fact]
        public async Task RetrievePostingAccounts_InvalidAccountType_ThrowsException()
        {
            // Arrange
            var member = new Member { Id = "mem-123" };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                _accountManager.RetrievePostingAccounts((AccountType)99, member));
        }

        [Fact]
        public async Task RetrievePostingAccounts_EmployerSponsored_NoEmployerId_ThrowsException()
        {
            // Arrange
            var member = new Member { Id = "mem-123", EmployerId = null };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _accountManager.RetrievePostingAccounts(AccountType.EmployerSponsoredPension, member));

            Assert.Equal("EmployerId cannot be null", ex.ParamName);
        }

        [Fact]
        public async Task RetrieveAllAccountsAsync_ReturnsPaginatedAccounts()
        {
            // Arrange
            var accounts = new List<Account>
        {
            new Account { AccountId = "acc-1" },
            new Account { AccountId = "acc-2" }
        };

            var paginatedAccounts = new PaginatedList<Account>(accounts, 2, 1, 10);
            var mappedAccounts = new PaginatedList<AccountDTO>(
                new List<AccountDTO>
                {
                new AccountDTO { PensionAccountId = "acc-1" },
                new AccountDTO { PensionAccountId = "acc-2" }
                },
                2, 1, 10);

            A.CallTo(() => _accountRepository.GetAll(
        A<int>._,
        A<int>._,
        A<Expression<Func<Account, bool>>>._,
        A<Expression<Func<Account, object>>>._))
    .Returns(paginatedAccounts);

            A.CallTo(() => _mapper.Map<PaginatedList<AccountDTO>>(paginatedAccounts))
                .Returns(mappedAccounts);

            A.CallTo(() => _cacheService.GetOrSetCacheAsync(
                    A<string>._,
                    A<Func<Task<IDataResult<PaginatedList<AccountDTO>>>>>._))
                .ReturnsLazily(async call =>
                {
                    var func = call.GetArgument<Func<Task<IDataResult<PaginatedList<AccountDTO>>>>>(1);
                    return await func();
                });

            // Act
            var result = await _accountManager.RetrieveAllAccountsAsync(1, 10);

            // Assert
            Assert.Equal(ResponseCode_Success, result.ResponseCode);
            Assert.Equal(2, result.Data.Items.Count);
        }

        [Fact]
        public async Task RetrieveAccountByIdAsync_ValidId_ReturnsAccount()
        {
            // Arrange
            var accountId = "acc-123";
            var account = new Account { AccountId = accountId };
            var accountDto = new AccountDTO { PensionAccountId = accountId };

            A.CallTo(() => _accountRepository.Get(A<Expression<Func<Account, bool>>>._))
                .Returns(account);

            A.CallTo(() => _mapper.Map<AccountDTO>(account))
                .Returns(accountDto);

            A.CallTo(() => _cacheService.GetOrSetCacheAsync(
                    A<string>._,
                    A<Func<Task<AccountDTO>>>._))
                .ReturnsLazily(async call =>
                {
                    var func = call.GetArgument<Func<Task<AccountDTO>>>(1);
                    return await func();
                });

            // Act
            var result = await _accountManager.RetrieveAccountByIdAsync(accountId);

            // Assert
            Assert.Equal(ResponseCode_Success, result.ResponseCode);
            Assert.Equal(accountId, result.Data.PensionAccountId);
        }

        [Fact]
        public async Task RetrieveAccountByIdAsync_InvalidId_ReturnsError()
        {
            // Arrange
            var accountId = "invalid-id";

            A.CallTo(() => _accountRepository.Get(A<Expression<Func<Account, bool>>>._))
                .Returns((Account)null);

            A.CallTo(() => _cacheService.GetOrSetCacheAsync(
                    A<string>._,
                    A<Func<Task<AccountDTO>>>._))
                .ReturnsLazily(async call =>
                {
                    var func = call.GetArgument<Func<Task<AccountDTO>>>(1);
                    return await func();
                });

            // Act
            var result = await _accountManager.RetrieveAccountByIdAsync(accountId);

            // Assert
            Assert.Equal(ResponseCode_AccountNotFound, result.ResponseCode);
        }

        [Fact]
        public async Task AddNewAccountAsync_ValidRequest_CreatesAccount()
        {
            // Arrange
            var request = new NewAccountRequest
            {
                MemberId = "mem-123",
                AccountType = AccountType.IndividualContribution
            };

            var member = new Member { Id = "mem-123" };
            var accountDto = new AccountDTO { PensionAccountId = "acc-123" };

            A.CallTo(() => _memberRepository.Get(A<Expression<Func<Member, bool>>>._))
                .Returns(member);

            A.CallTo(() => _mapper.Map<AccountDTO>(A<Account>._))
                .Returns(accountDto);

            // Act
            var result = await _accountManager.AddNewAccountAsync(request);

            // Assert
            Assert.Equal(ResponseCode_Success, result.ResponseCode);
            A.CallTo(() => _accountRepository.AddAsync(A<Account>._)).MustHaveHappened();
            A.CallTo(() => _unitOfWork.SaveChangesAsync()).MustHaveHappened();
            A.CallTo(() => _unitOfWork.CommitAsync()).MustHaveHappened();
        }

        [Fact]
        public async Task AddNewAccountAsync_MemberNotFound_ReturnsError()
        {
            // Arrange
            var request = new NewAccountRequest
            {
                MemberId = "invalid-id",
                AccountType = AccountType.IndividualContribution
            };

            A.CallTo(() => _memberRepository.Get(A<Expression<Func<Member, bool>>>._))
                .Returns((Member)null);

            // Act
            var result = await _accountManager.AddNewAccountAsync(request);

            // Assert
            Assert.Equal(ResponseCode_MemberNotFound, result.ResponseCode);
        }

        [Fact]
        public async Task GetAccountsForSpecificMemberAsync_ValidMember_ReturnsAccounts()
        {
            // Arrange
            var memberId = "mem-123";
            var accounts = new List<Account>
        {
            new Account { AccountId = "acc-1", MemberId = memberId }
        };

            var paginatedAccounts = new PaginatedList<Account>(accounts, 1, 1, 10);
            var mappedAccounts = new PaginatedList<AccountDTO>(
                new List<AccountDTO>
                {
                new AccountDTO { PensionAccountId = "acc-1", MemberId = memberId }
                },
                1, 1, 10);

            // Correct mock when the 4th parameter is Expression<Func<Account, object>>
            A.CallTo(() => _accountRepository.GetAll(
                    A<int>._,                      // pageNumber
                    A<int>._,                      // pageSize
                    A<Expression<Func<Account, bool>>>._,  // filter
                    A<Expression<Func<Account, object>>>._))  // include
                .Returns(paginatedAccounts);

            A.CallTo(() => _mapper.Map<PaginatedList<AccountDTO>>(paginatedAccounts))
                .Returns(mappedAccounts);

            A.CallTo(() => _cacheService.GetOrSetCacheAsync(
                    A<string>._,
                    A<Func<Task<IDataResult<PaginatedList<AccountDTO>>>>>._))
                .ReturnsLazily(async call =>
                {
                    var func = call.GetArgument<Func<Task<IDataResult<PaginatedList<AccountDTO>>>>>(1);
                    return await func();
                });

            // Act
            var result = await _accountManager.GetAccountsForSpecificMemberAsync(memberId);

            // Assert
            Assert.Equal(ResponseCode_Success, result.ResponseCode);
            Assert.Equal(memberId, result.Data.Items.First().MemberId);
        }

        [Fact]
        public async Task GetAccountsForSpecificMemberAsync_InvalidMemberId_ReturnsError()
        {
            // Arrange
            var memberId = "";

            // Act
            var result = await _accountManager.GetAccountsForSpecificMemberAsync(memberId);

            // Assert
            Assert.Equal("14", result.ResponseCode);
        }
    }
}
