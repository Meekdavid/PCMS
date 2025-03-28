using API.Controllers;
using Application.Interfaces.Business;
using Common.DTOs.Requests;
using Common.DTOs.Responses;
using Common.Models;
using Common.Pagination;
using Core.Results;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Persistence.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests.Shared;
using Xunit;
using static Common.Literals.StringLiterals;

namespace Tests.API.AccountControllerTests
{
    public class AccountControllerTest
    {
        private readonly IAccountManager _accountManager;
        private readonly AccountController _controller;

        public AccountControllerTest()
        {
            _accountManager = A.Fake<IAccountManager>();
            _controller = new AccountController(_accountManager);
        }

        [Fact]
        public async Task CreateAccount_ValidRequest_ReturnsSuccess()
        {
            // Arrange
            var request = new NewAccountRequest
            {
                MemberId = "mem-123",
                AccountType = AccountType.IndividualContribution
            };

            var fakeAccount = A.Fake<AccountDTO>();
            fakeAccount.PensionAccountId = "acc-123";
            fakeAccount.MemberId = "mem-456";
            fakeAccount.Balance = 5000.00m;

            var expectedResult = new SuccessDataResult<AccountDTO>(fakeAccount);

            A.CallTo(() => _accountManager.AddNewAccountAsync(request))
                .Returns(Task.FromResult<IDataResult<AccountDTO>>(expectedResult));

            // Act
            var result = await _controller.CreateAccount(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dataResult = Assert.IsType<SuccessDataResult<string>>(okResult.Value);
            Assert.Equal(ResponseCode_Success, dataResult.ResponseCode);
        }

        [Fact]
        public async Task CreateAccount_MemberNotFound_ReturnsError()
        {
            // Arrange
            var request = new NewAccountRequest
            {
                MemberId = "invalid",
                AccountType = AccountType.EmployerSponsoredPension
            };

            var expectedResult = new ErrorDataResult<AccountDTO>(
                null,
                ResponseCode_MemberNotFound,
                ResponseMessage_MemberNotFound
            );

            A.CallTo(() => _accountManager.AddNewAccountAsync(request))
                .Returns(Task.FromResult<IDataResult<AccountDTO>>(expectedResult));

            // Act
            var result = await _controller.CreateAccount(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dataResult = Assert.IsType<ErrorDataResult<string>>(okResult.Value);
            Assert.Equal(ResponseCode_MemberNotFound, dataResult.ResponseCode);
        }

        [Fact]
        public async Task GetById_ValidId_ReturnsAccount()
        {
            // Arrange
            var accountId = "acc-123";
            var accountDto = new AccountDTO
            {
                PensionAccountId = accountId
            };

            var expectedResult = new SuccessDataResult<AccountDTO>(
                accountDto,
                ResponseCode_Success
            );

            A.CallTo(() => _accountManager.RetrieveAccountByIdAsync(accountId))
                .Returns(Task.FromResult<IDataResult<AccountDTO>>(expectedResult));

            // Act
            var result = await _controller.GetById(accountId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dataResult = Assert.IsType<SuccessDataResult<AccountDTO>>(okResult.Value);
            Assert.Equal(accountId, dataResult.Data.PensionAccountId);
        }

        [Fact]
        public async Task GetById_AccountNotFound_ReturnsError()
        {
            // Arrange
            var accountId = "invalid";

            var expectedResult = new ErrorDataResult<AccountDTO>(
                null!,
                ResponseCode_AccountNotFound,
                ResponseMessage_AccountNotFound
            );

            A.CallTo(() => _accountManager.RetrieveAccountByIdAsync(accountId))
                .Returns(Task.FromResult<IDataResult<AccountDTO>>(expectedResult));

            // Act
            var result = await _controller.GetById(accountId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dataResult = Assert.IsType<ErrorDataResult<AccountDTO>>(okResult.Value);
            Assert.Equal(ResponseCode_AccountNotFound, dataResult.ResponseCode);
        }

        [Fact]
        public async Task GetAll_ReturnsPaginatedAccounts()
        {
            // Arrange
            var accounts = new List<AccountDTO>
                {
                    new AccountDTO { PensionAccountId = "acc-1" },
                    new AccountDTO { PensionAccountId = "acc-2" }
                };

            var paginatedList = new PaginatedList<AccountDTO>(accounts, accounts.Count, 1, 10);

            var expectedResult = new SuccessDataResult<PaginatedList<AccountDTO>>(
                paginatedList,
                ResponseCode_Success
            );

            A.CallTo(() => _accountManager.RetrieveAllAccountsAsync(1, 10))
                .Returns(Task.FromResult<IDataResult<PaginatedList<AccountDTO>>>(expectedResult));

            // Act
            var result = await _controller.GetAll(1, 10);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dataResult = Assert.IsType<SuccessDataResult<PaginatedList<AccountDTO>>>(okResult.Value);
            Assert.Equal(2, dataResult.Data.Items.Count);
        }

        [Fact]
        public async Task GetAccountsForMember_ValidMember_ReturnsAccounts()
        {
            // Arrange
            var memberId = "mem-123";
            var accounts = new List<AccountDTO>
                {
                    new AccountDTO { MemberId = memberId }
                };

            var paginatedList = new PaginatedListBuilder<AccountDTO>()
                .WithItems(accounts)
                .WithTotalCount(15)
                .WithPageIndex(1)
                .WithPageSize(3)
                .Build();

            var expectedResult = new SuccessDataResult<PaginatedList<AccountDTO>>(paginatedList);

            A.CallTo(() => _accountManager.GetAccountsForSpecificMemberAsync(memberId))
                .Returns(Task.FromResult<IDataResult<PaginatedList<AccountDTO>>>(expectedResult));

            // Act
            var result = await _controller.GetAccountsForMember(memberId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dataResult = Assert.IsType<SuccessDataResult<List<AccountDTO>>>(okResult.Value);
            Assert.Equal(memberId, dataResult.Data.First().MemberId);
        }

        [Fact]
        public async Task GetAccountsForMember_NoAccounts_ReturnsError()
        {
            // Arrange
            var memberId = "mem-123";

            var expectedResult = new ErrorDataResult<PaginatedList<AccountDTO>>(
                ResponseCode_MemberNotFound,
                ResponseMessage_AccountNotFound
            );

            A.CallTo(() => _accountManager.GetAccountsForSpecificMemberAsync(memberId))
                .Returns(Task.FromResult<IDataResult<PaginatedList<AccountDTO>>>(expectedResult));

            // Act
            var result = await _controller.GetAccountsForMember(memberId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dataResult = Assert.IsType<ErrorDataResult<List<AccountDTO>>>(okResult.Value);
            Assert.Equal(ResponseCode_MemberNotFound, dataResult.ResponseCode);
        }
    }
}
