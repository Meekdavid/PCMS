using API.Controllers;
using Application.Interfaces.General;
using Common.DTOs.Requests;
using Common.DTOs.Responses;
using Common.Models;
using Common.Pagination;
using Core.Results;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Persistence.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static Common.Literals.StringLiterals;

namespace Tests.API.MemberControllerTests
{
    public class MemberControllerTest
    {
        private readonly IUserManager _memberManager;
        private readonly MemberController _controller;
        private readonly Mock<HttpContext> _mockHttpContext;

        public MemberControllerTest()
        {
            _memberManager = A.Fake<IUserManager>();
            _mockHttpContext = new Mock<HttpContext>();
            _controller = new MemberController(_memberManager)
            {
                ControllerContext = new ControllerContext { HttpContext = _mockHttpContext.Object }
            };
        }

        [Fact]
        public async Task Register_ValidRequest_ReturnsSuccess()
        {
            // Arrange
            var request = new MemberRequest
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com"
            };

            var expectedResult = new SuccessDataResult<string>("Member created");

            A.CallTo(() => _memberManager.CreateMemberAndAssignRolesAsync(
                    request,
                    A<List<string>>.That.Matches(x => x.Contains("Member")),
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
        public async Task Register_DuplicateMember_ReturnsConflict()
        {
            // Arrange
            var request = new MemberRequest
            {
                FirstName = "Existing",
                LastName = "User",
                Email = "existing@example.com"
            };

            var expectedResult = new ErrorDataResult<string>("34", "Member already exists");

            A.CallTo(() => _memberManager.CreateMemberAndAssignRolesAsync(
                    request,
                    A<List<string>>._,
                    _mockHttpContext.Object))
                .Returns(Task.FromResult<IDataResult<string>>(expectedResult));

            // Act
            var result = await _controller.Register(request);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var dataResult = Assert.IsType<ErrorDataResult<string>>(conflictResult.Value);
            Assert.Equal("34", dataResult.ResponseCode);
        }

        [Fact]
        public async Task Update_ValidRequest_ReturnsSuccess()
        {
            // Arrange
            var memberId = "mem-123";
            var request = new MemberUpdateRequest
            {
                FirstName = "Updated",
                LastName = "Name"
            };

            var expectedResult = new SuccessDataResult<string>("Member updated", ResponseCode_Success);

            A.CallTo(() => _memberManager.UpdateMemberAsync(
                    memberId,
                    request,
                    _mockHttpContext.Object))
                .Returns(Task.FromResult<IDataResult<string>>(expectedResult));

            // Act
            var result = await _controller.Update(memberId, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dataResult = Assert.IsType<SuccessDataResult<string>>(okResult.Value);
            Assert.Equal(ResponseCode_Success, dataResult.ResponseCode);
        }

        [Fact]
        public async Task RetrieveMemberById_ValidId_ReturnsMember()
        {
            // Arrange
            var memberId = "mem-123";
            var memberDto = new MemberDTO
            {
                MemberId = memberId,
                FirstName = "John",
                LastName = "Doe"
            };

            var expectedResult = new SuccessDataResult<MemberDTO>(memberDto, ResponseCode_Success);

            A.CallTo(() => _memberManager.RetrieveMemberById(memberId))
                .Returns(Task.FromResult<IDataResult<MemberDTO>>(expectedResult));

            // Act
            var result = await _controller.RetrieveMemberById(memberId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dataResult = Assert.IsType<SuccessDataResult<MemberDTO>>(okResult.Value);
            Assert.Equal(memberId, dataResult.Data.MemberId);
        }

        [Fact]
        public async Task RetrieveAllMembers_ReturnsPaginatedMembers()
        {
            // Arrange
            var members = new List<MemberDTO>
                {
                    new MemberDTO { MemberId = "mem-1" },
                    new MemberDTO { MemberId = "mem-2" }
                };

            var paginatedList = new PaginatedList<MemberDTO>(members, 2, 1, 10);
            var expectedResult = new SuccessDataResult<PaginatedList<MemberDTO>>(paginatedList, ResponseCode_Success);

            A.CallTo(() => _memberManager.RetrieveAllMembers(1, 10))
                .Returns(Task.FromResult<IDataResult<PaginatedList<MemberDTO>>>(expectedResult));

            // Act
            var result = await _controller.RetrieveAllMembers(1, 10);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
        }
        [Fact]
        public async Task RetrieveAllMembersByMemberType_EmployeeType_ReturnsFilteredList()
        {
            // Arrange
            var members = new List<MemberDTO>
        {
            new MemberDTO { MemberId = "mem-1", MembershipType = MembershipType.Employee }
        };

            var paginatedList = new PaginatedList<MemberDTO>(members, 1, 1, 10);
            var expectedResult = new SuccessDataResult<PaginatedList<MemberDTO>>(paginatedList, ResponseCode_Success);

            A.CallTo(() => _memberManager.RetrieveMemberByType(1, 10, MembershipType.Employee))
                .Returns(Task.FromResult<IDataResult<PaginatedList<MemberDTO>>>(expectedResult));

            // Act
            var result = await _controller.RetrieveAllMembersbyMemberType(MembershipType.Employee, 1, 10);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dataResult = Assert.IsType<SuccessDataResult<PaginatedList<MemberDTO>>>(okResult.Value);
            Assert.Single(dataResult.Data.Items);
            Assert.Equal(MembershipType.Employee, dataResult.Data.Items.First().MembershipType);
        }

        [Fact]
        public async Task DeleteMember_ValidId_ReturnsSuccess()
        {
            // Arrange
            var memberId = "mem-123";
            var expectedResult = new SuccessResult("Member deleted");

            A.CallTo(() => _memberManager.SoftDeleteMemberAsync(memberId))
                .Returns(Task.FromResult<Core.Results.IResult>(expectedResult));

            // Act
            var result = await _controller.DeleteMember(memberId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dataResult = Assert.IsType<SuccessDataResult<string>>(okResult.Value);
            Assert.Equal(ResponseCode_Success, dataResult.ResponseCode);
        }

        [Fact]
        public async Task DeleteMember_MemberNotFound_ReturnsError()
        {
            // Arrange
            var memberId = "invalid-id";
            var expectedResult = new ErrorResult("Member not found");

            A.CallTo(() => _memberManager.SoftDeleteMemberAsync(memberId))
                .Returns(Task.FromResult<Core.Results.IResult>(expectedResult));

            // Act
            var result = await _controller.DeleteMember(memberId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dataResult = Assert.IsType<ErrorDataResult<string>>(okResult.Value);
            Assert.Equal(ResponseCode_MemberNotFound, dataResult.ResponseCode);
        }

        [Fact]
        public async Task Register_InvalidInput_ReturnsUnprocessableEntity()
        {
            // Arrange
            var request = new MemberRequest(); // Missing required fields
            _controller.ModelState.AddModelError("FirstName", "Required");

            // Act
            var result = await _controller.Register(request);

            // Assert
            Assert.IsType<UnprocessableEntityObjectResult>(result);
        }

        [Fact]
        public async Task RetrieveMemberById_NotFound_ReturnsError()
        {
            // Arrange
            var memberId = "invalid-id";
            var expectedResult = new ErrorDataResult<MemberDTO>("Member not found", ResponseCode_MemberNotFound);

            A.CallTo(() => _memberManager.RetrieveMemberById(memberId))
                .Returns(Task.FromResult<IDataResult<MemberDTO>>(expectedResult));

            // Act
            var result = await _controller.RetrieveMemberById(memberId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dataResult = Assert.IsType<ErrorDataResult<MemberDTO>>(okResult.Value);
            Assert.Equal(ResponseCode_MemberNotFound, dataResult.ResponseCode);
        }
    }
}
