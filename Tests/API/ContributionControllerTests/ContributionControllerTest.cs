using API.Controllers;
using Application.Interfaces.Business;
using Common.DTOs.Requests;
using Common.DTOs.Responses;
using Common.Models;
using Core.Results;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Persistence.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests.Shared;
using Xunit;
using static Common.Literals.StringLiterals;

namespace Tests.API.ContributionControllerTests
{
    public class ContributionControllerTest
    {
        private readonly IContributionManager _contributionManager;
        private readonly ContributionController _controller;

        public ContributionControllerTest()
        {
            _contributionManager = A.Fake<IContributionManager>();
            _controller = new ContributionController(_contributionManager);
        }

        [Fact]
        public async Task AddContribution_ValidRequest_ReturnsSuccess()
        {
            // Arrange
            var request = new ContributionRequest
            {
                Amount = 1000.00m,
                ContributionType = ContributionType.Monthly,
                MemberId = "acc-123"
            };

            var contributionDto = new ContributionDTO
            {
                ContributionId = "cont-123",
                Amount = request.Amount,
                ContributionType = request.ContributionType
            };

            var expectedResult = new SuccessDataResult<ContributionDTO>(contributionDto, ResponseCode_Success);

            A.CallTo(() => _contributionManager.AddContributionAsync(request))
                .Returns(Task.FromResult<IDataResult<ContributionDTO>>(expectedResult));

            // Act
            var result = await _controller.AddContribution(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dataResult = Assert.IsType<SuccessDataResult<ContributionDTO>>(okResult.Value);
            Assert.Equal(ResponseCode_Success, dataResult.ResponseCode);
            Assert.Equal(1000.00m, dataResult.Data.Amount);
        }

        [Fact]
        public async Task AddContribution_InvalidAmount_ReturnsUnprocessableEntity()
        {
            // Arrange
            var request = new ContributionRequest { Amount = 0 }; // Invalid amount
            _controller.ModelState.AddModelError("Amount", "Amount must be greater than 0");

            // Act
            var result = await _controller.AddContribution(request);

            // Assert
            Assert.IsType<UnprocessableEntityObjectResult>(result);
        }

        [Fact]
        public async Task GetContributionById_ValidId_ReturnsContribution()
        {
            // Arrange
            var contributionId = "cont-123";
            var contributionDto = new ContributionDTO
            {
                ContributionId = contributionId,
                Amount = 500.00m
            };

            var expectedResult = new SuccessDataResult<ContributionDTO>(contributionDto, ResponseCode_Success);

            A.CallTo(() => _contributionManager.GetContributionByIdAsync(contributionId))
                .Returns(Task.FromResult<IDataResult<ContributionDTO>>(expectedResult));

            // Act
            var result = await _controller.GetContributionById(contributionId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dataResult = Assert.IsType<SuccessDataResult<ContributionDTO>>(okResult.Value);
            Assert.Equal(contributionId, dataResult.Data.ContributionId);
        }

        [Fact]
        public async Task GetContributionById_NotFound_ReturnsError()
        {
            // Arrange
            var contributionId = "invalid-id";
            var expectedResult = new ErrorDataResult<ContributionDTO>(null, ResponseCode_ContributionNotFound, ResponseMessage_ContributionNotFound);

            A.CallTo(() => _contributionManager.GetContributionByIdAsync(contributionId))
                .Returns(Task.FromResult<IDataResult<ContributionDTO>>(expectedResult));

            // Act
            var result = await _controller.GetContributionById(contributionId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dataResult = Assert.IsType<ErrorDataResult<ContributionDTO>>(okResult.Value);
            Assert.Equal(ResponseCode_ContributionNotFound, dataResult.ResponseCode);
        }

        [Fact]
        public async Task GetContributionsByMember_ValidMember_ReturnsContributions()
        {
            // Arrange
            var memberId = "mem-123";
            var contributions = new List<ContributionDTO>
        {
            new ContributionDTO { MemberId = memberId, Amount = 1000.00m },
            new ContributionDTO { MemberId = memberId, Amount = 2000.00m }
        };

            var expectedResult = new SuccessDataResult<List<ContributionDTO>>(contributions, ResponseCode_Success);

            A.CallTo(() => _contributionManager.GetContributionsByMemberAsync(memberId))
                .Returns(Task.FromResult<IDataResult<List<ContributionDTO>>>(expectedResult));

            // Act
            var result = await _controller.GetContributionsByMember(memberId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dataResult = Assert.IsType<SuccessDataResult<List<ContributionDTO>>>(okResult.Value);
            Assert.Equal(2, dataResult.Data.Count);
            Assert.Equal(memberId, dataResult.Data.First().MemberId);
        }

        [Fact]
        public async Task GetContributionSummary_ValidMember_ReturnsSummary()
        {
            // Arrange
            var memberId = "mem-123";
            var summary = new ContributionSummaryDTO
            {
                MemberId = memberId,
                ContributionCount = 500
            };

            var expectedResult = new SuccessDataResult<ContributionSummaryDTO>(summary, ResponseCode_Success);

            A.CallTo(() => _contributionManager.GetContributionSummaryAsync(memberId))
                .Returns(Task.FromResult<IDataResult<ContributionSummaryDTO>>(expectedResult));

            // Act
            var result = await _controller.GetContributionSummary(memberId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dataResult = Assert.IsType<SuccessDataResult<ContributionSummaryDTO>>(okResult.Value);
            Assert.Equal(5000.00m, dataResult.Data.ContributionCount);
        }

        [Fact]
        public async Task GenerateStatements_ValidRequest_ReturnsFile()
        {
            // Arrange
            var request = new StatementRequest
            {
                MemberId = "mem-123",
                StartDate = DateTime.Now.AddMonths(-1),
                EndDate = DateTime.Now
            };

            // Create a sample PDF content (in a real test, this would be your actual statement)
            var fileContent = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // %PDF header
            var statementResult = new StatementResult
            {
                Content = fileContent,
                ContentType = "application/pdf",
                FileName = "statement_2023.pdf"
            };

            var expectedResult = new SuccessDataResult<StatementResult>(statementResult, ResponseCode_Success);

            A.CallTo(() => _contributionManager.GenerateStatementsAsync(request))
                .Returns(Task.FromResult<IDataResult<StatementResult>>(expectedResult));

            // Act
            var result = await _controller.GenerateStatements(request);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result); // Changed to FileContentResult
            Assert.Equal("application/pdf", fileResult.ContentType);
            Assert.Equal("statement_2023.pdf", fileResult.FileDownloadName);
            Assert.Equal(fileContent, fileResult.FileContents);
        }

        [Fact]
        public async Task ProcessWithdrawal_EligibleMember_ReturnsSuccess()
        {
            // Arrange
            var request = new WithdrawalRequest
            {
                MemberId = "mem-123",
                Amount = 1000.00m
            };

            var withdrawalResult = new WithdrawalResult
            {
                TransactionId = "txn-123",
                NewBalance = 4000.00m
            };

            var expectedResult = new SuccessDataResult<WithdrawalResult>(withdrawalResult, ResponseCode_Success);

            A.CallTo(() => _contributionManager.ProcessWithdrawalAsync(request))
                .Returns(Task.FromResult<IDataResult<WithdrawalResult>>(expectedResult));

            // Act
            var result = await _controller.ProcessWithdrawal(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dataResult = Assert.IsType<SuccessDataResult<WithdrawalResult>>(okResult.Value);
            Assert.Equal("txn-123", dataResult.Data.TransactionId);
        }

        [Fact]
        public async Task ProcessWithdrawal_IneligibleMember_ReturnsForbidden()
        {
            // Arrange
            var request = new WithdrawalRequest
            {
                MemberId = "mem-123",
                Amount = 1000.00m
            };

            var expectedResult = new ErrorDataResult<WithdrawalResult>(
                ResponseCode_MemberNotEligibleForBenefits,
                ResponseMessage_MemberNotEligibleForBenefits
            );

            A.CallTo(() => _contributionManager.ProcessWithdrawalAsync(request))
                .Returns(Task.FromResult<IDataResult<WithdrawalResult>>(expectedResult));

            // Act
            var result = await _controller.ProcessWithdrawal(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var dataResult = Assert.IsType<ErrorDataResult<WithdrawalResult>>(badRequestResult.Value);
            Assert.Equal(ResponseCode_MemberNotEligibleForBenefits, dataResult.ResponseCode);
        }

        [Fact]
        public async Task CheckEligibility_EligibleMember_ReturnsSuccess()
        {
            // Arrange
            var memberId = "mem-123";
            var eligibilityResult = new EligibilityResultDTO
            {
                IsEligible = true,
            };

            var expectedResult = new SuccessDataResult<EligibilityResultDTO>(eligibilityResult, ResponseCode_Success);

            A.CallTo(() => _contributionManager.CheckEligibilityAsync(memberId))
                .Returns(Task.FromResult<IDataResult<EligibilityResultDTO>>(expectedResult));

            // Act
            var result = await _controller.CheckEligibility(memberId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dataResult = Assert.IsType<SuccessDataResult<EligibilityResultDTO>>(okResult.Value);
            Assert.True(dataResult.Data.IsEligible);
        }
    }
}
