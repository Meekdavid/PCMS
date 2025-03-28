using API.Controllers;
using Application.Interfaces.Business;
using Common.DTOs.Requests;
using Common.DTOs.Responses;
using Common.Models;
using Common.Pagination;
using Core.Results;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static Common.Literals.StringLiterals;

namespace Tests.API.EmployerControllerTests
{
    public class EmployerControllerTest
    {
        private readonly IEmployerManager _employerManager;
        private readonly EmployerController _controller;
        private readonly Mock<HttpContext> _mockHttpContext;

        public EmployerControllerTest()
        {
            _employerManager = A.Fake<IEmployerManager>();
            _mockHttpContext = new Mock<HttpContext>();
            _controller = new EmployerController(_employerManager)
            {
                ControllerContext = new ControllerContext { HttpContext = _mockHttpContext.Object }
            };
        }

        [Fact]
        public async Task Register_ValidRequest_ReturnsSuccess()
        {
            // Arrange
            var request = new EmployerRequest
            {
                CompanyName = "Test Corp",
                RegistrationNumber = "REG123",
                IsActive = true
            };

            var expectedResult = new SuccessDataResult<string>("Employer created");

            A.CallTo(() => _employerManager.CreateEmployerAsync(
                    request,
                    new List<string> { "Employer" },
                    _mockHttpContext.Object))
                .Returns(Task.FromResult<IDataResult<string>>(expectedResult));

            // Act
            var result = await _controller.Register(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dataResult = Assert.IsType<SuccessDataResult<string>>(okResult.Value);
            Assert.Equal(ResponseCode_Success, dataResult.ResponseCode);
        }

        [Fact]
        public async Task Register_DuplicateEmployer_ReturnsConflict()
        {
            // Arrange
            var request = new EmployerRequest
            {
                CompanyName = "Existing Corp",
                RegistrationNumber = "EXIST123"
            };

            var expectedResult = new ErrorDataResult<string>(ResponseCode_EmployerAlreadyExists, ResponseMessage_EmployerAlreadyExists);

            A.CallTo(() => _employerManager.CreateEmployerAsync(
                    request,
                    A<List<string>>._,
                    _mockHttpContext.Object))
                .Returns(Task.FromResult<IDataResult<string>>(expectedResult));

            // Act
            var result = await _controller.Register(request);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var dataResult = Assert.IsType<ErrorDataResult<string>>(conflictResult.Value);
            Assert.Equal(ResponseCode_EmployerAlreadyExists, dataResult.ResponseCode);
        }

        [Fact]
        public async Task Update_ValidRequest_ReturnsSuccess()
        {
            // Arrange
            var employerId = "emp-123";
            var request = new EmployerUpdateRequest
            {
                CompanyName = "Updated Corp",
                RegistrationNumber = "UPD123"
            };

            var expectedResult = new SuccessDataResult<string>("Employer updated");

            A.CallTo(() => _employerManager.UpdateEmployerAsync(
                    employerId,
                    request,
                    _mockHttpContext.Object))
                .Returns(Task.FromResult<IDataResult<string>>(expectedResult));

            // Act
            var result = await _controller.Update(employerId, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dataResult = Assert.IsType<SuccessDataResult<string>>(okResult.Value);
            Assert.Equal(ResponseCode_Success, dataResult.ResponseCode);
        }

        [Fact]
        public async Task GetById_ValidId_ReturnsEmployer()
        {
            // Arrange
            var employerId = "emp-123";
            var employerDto = new EmployerDTO
            {
                EmployerId = employerId,
                CompanyName = "Test Corp"
            };

            var expectedResult = new SuccessDataResult<EmployerDTO>(employerDto, ResponseCode_Success);

            A.CallTo(() => _employerManager.RetrieveEmployerByIdAsync(employerId))
                .Returns(Task.FromResult<IDataResult<EmployerDTO>>(expectedResult));

            // Act
            var result = await _controller.GetById(employerId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dataResult = Assert.IsType<SuccessDataResult<EmployerDTO>>(okResult.Value);
            Assert.Equal(employerId, dataResult.Data.EmployerId);
        }

        [Fact]
        public async Task GetAll_ReturnsPaginatedEmployers()
        {
            // Arrange
            var employers = new List<EmployerDTO>
                {
                    new EmployerDTO { EmployerId = "emp-1" },
                    new EmployerDTO { EmployerId = "emp-2" }
                };

            var paginatedList = new PaginatedList<EmployerDTO>(employers, 2, 1, 10);
            var expectedResult = new SuccessDataResult<PaginatedList<EmployerDTO>>(paginatedList, ResponseCode_Success);

            A.CallTo(() => _employerManager.RetrieveAllEmployersAsync(1, 10))
                .Returns(Task.FromResult<IDataResult<PaginatedList<EmployerDTO>>>(expectedResult));

            // Act
            var result = await _controller.GetAll(1, 10);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dataResult = Assert.IsType<SuccessDataResult<PaginatedList<EmployerDTO>>>(okResult.Value);
            Assert.Equal(2, dataResult.Data.Items.Count);
        }

        [Fact]
        public async Task GetByStatus_ActiveEmployers_ReturnsFilteredList()
        {
            // Arrange
            var employers = new List<EmployerDTO>
                {
                    new EmployerDTO { EmployerId = "emp-1" }
                };

            var paginatedList = new PaginatedList<EmployerDTO>(employers, 1, 1, 10);
            var expectedResult = new SuccessDataResult<PaginatedList<EmployerDTO>>(paginatedList, ResponseCode_Success);

            A.CallTo(() => _employerManager.RetrieveEmployersByStatusAsync(1, 10, true))
                .Returns(Task.FromResult<IDataResult<PaginatedList<EmployerDTO>>>(expectedResult));

            // Act
            var result = await _controller.GetByStatus(true, 1, 10);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dataResult = Assert.IsType<SuccessDataResult<PaginatedList<EmployerDTO>>>(okResult.Value);
            Assert.Single(dataResult.Data.Items);
        }

        [Fact]
        public async Task Delete_ValidId_ReturnsSuccess()
        {
            // Arrange
            var employerId = "emp-123";
            var expectedResult = new SuccessResult("Employer deleted");

            A.CallTo(() => _employerManager.SoftDeleteEmployerAsync(employerId))
                .Returns(Task.FromResult<Core.Results.IResult>(expectedResult));

            // Act
            var result = await _controller.Delete(employerId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dataResult = Assert.IsType<SuccessResult>(okResult.Value);
            Assert.Equal(ResponseCode_Success, dataResult.ResponseCode);
        }

        [Fact]
        public async Task Delete_EmployerNotFound_ReturnsError()
        {
            // Arrange
            var employerId = "invalid-id";
            var expectedResult = new ErrorResult(ResponseCode_EmployerNotFound, ResponseMessage_EmployerNotFound);

            A.CallTo(() => _employerManager.SoftDeleteEmployerAsync(employerId))
                .Returns(Task.FromResult<Core.Results.IResult>(expectedResult));

            // Act
            var result = await _controller.Delete(employerId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dataResult = Assert.IsType<ErrorDataResult<string>>(okResult.Value);
            Assert.Equal(ResponseCode_EmployerNotFound, dataResult.ResponseCode);
        }

        [Fact]
        public async Task Register_InvalidInput_ReturnsUnprocessableEntity()
        {
            // Arrange
            var request = new EmployerRequest(); // Missing required fields
            _controller.ModelState.AddModelError("CompanyName", "Required");

            // Act
            var result = await _controller.Register(request);

            // Assert
            Assert.IsType<UnprocessableEntityObjectResult>(result);
        }
    }
}
