using Application.Interfaces.Database;
using Application.Interfaces.General;
using Application.Repositories;
using AutoMapper;
using Common.DTOs.Requests;
using Common.DTOs.Responses;
using Common.Models;
using Common.Pagination;
using Common.Services;
using Core.Results;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
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

namespace Tests.BusinessLogics.MemberManagerTests
{
    public class MemberManagerTest
    {
        private readonly UserManager<Member> _userManager;
        private readonly ILogger<MemberManager> _logger;
        private readonly ITokenHandler _tokenHandler;
        private readonly ICacheService _cacheService;
        private readonly IEmailServiceCustom _emailHandler;
        private readonly IMemberRepository _memberDal;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IStorageFactory _storageFactory;
        private readonly MemberManager _manager;

        public MemberManagerTest()
        {
            _userManager = A.Fake<UserManager<Member>>();
            _logger = A.Fake<ILogger<MemberManager>>();
            _tokenHandler = A.Fake<ITokenHandler>();
            _memberDal = A.Fake<IMemberRepository>();
            _unitOfWork = A.Fake<IUnitOfWork>();
            _mapper = A.Fake<IMapper>();
            _storageFactory = A.Fake<IStorageFactory>();
            _emailHandler = A.Fake<IEmailServiceCustom>();
            _cacheService = A.Fake<ICacheService>();

            _manager = new MemberManager(
                _userManager,
                _logger,
                _tokenHandler,
                _memberDal,
                _unitOfWork,
                _mapper,
                _storageFactory,
                _emailHandler,
                _cacheService);
        }

        [Fact]
        public async Task ChangeRefreshToken_ValidRequest_UpdatesToken()
        {
            // Arrange
            var request = new MemberChangeRefreshTokenRequest
            {
                MemberId = "mem-123",
                RefreshToken = "new-refresh-token",
                RefreshTokenEndDate = DateTime.UtcNow.AddDays(7)
            };

            var member = new Member { Id = "mem-123" };

            A.CallTo(() => _memberDal.GetById(request.MemberId))
                .Returns(Task.FromResult(member));

            // Act
            var result = await _manager.ChangeRefreshToken(request);

            // Assert
            Assert.Equal(ResponseCode_Success, result.ResponseCode);
            A.CallTo(() => _memberDal.Update(member)).MustHaveHappened();
            A.CallTo(() => _unitOfWork.CommitAsync()).MustHaveHappened();
        }

        [Fact]
        public async Task CreateAccessToken_ValidMember_ReturnsToken()
        {
            // Arrange
            var member = new Member { Id = "mem-123" };
            var token = new Token { AccessToken = "test-token" };
            var roles = new List<string> { "Member" };

            A.CallTo(() => _userManager.GetRolesAsync(member))
                .Returns(Task.FromResult((IList<string>)roles));

            A.CallTo(() => _tokenHandler.CreateAccessTokenAsync(member, roles))
                .Returns(Task.FromResult(token));

            // Act
            var result = await _manager.CreateAccessToken(member);

            // Assert
            Assert.Equal(ResponseCode_Success, result.ResponseCode);
            Assert.Equal(token.AccessToken, result.Data.AccessToken);
        }

        [Fact]
        public async Task SignInAsync_ValidCredentials_ReturnsToken()
        {
            // Arrange
            var email = "test@example.com";
            var password = "password";
            var member = new Member { Id = "mem-123", EmailConfirmed = true };
            var token = new Token { AccessToken = "test-token" };

            // Mock UserManager calls
            A.CallTo(() => _userManager.FindByEmailAsync(email))
                .Returns(Task.FromResult(member));

            A.CallTo(() => _userManager.CheckPasswordAsync(member, password))
                .Returns(Task.FromResult(true));

            // Mock TokenHandler (the actual dependency that creates tokens)
            A.CallTo(() => _tokenHandler.CreateAccessTokenAsync(member, A<List<string>>._))
                .Returns(Task.FromResult(token));

            // Mock UserManager roles
            A.CallTo(() => _userManager.GetRolesAsync(member))
                .Returns(Task.FromResult<IList<string>>(new List<string> { "Member" }));

            // Act - call the actual method you want to test
            var result = await _manager.SignInAsync(email, password);

            // Assert
            Assert.Equal(ResponseCode_Success, result.ResponseCode);
            Assert.Equal(token.AccessToken, result.Data.AccessToken);
        }

        [Fact]
        public async Task RetrieveAllMembers_ReturnsPaginatedMembers()
        {
            // Arrange
            var pageIndex = 1;
            var pageSize = 10;
            var members = new List<Member> { new Member { Id = "mem-123" } };
            var paginatedMembers = new PaginatedList<Member>(members, 1, pageIndex, pageSize);
            var mappedMembers = new PaginatedList<MemberDTO>(
                new List<MemberDTO> { new MemberDTO { MemberId = "mem-123" } },
                1, pageIndex, pageSize);

            A.CallTo(() => _memberDal.GetAll(
                    pageIndex,
                    pageSize,
                    null,
                    A<Expression<Func<Member, object>>>._))
                .Returns(Task.FromResult(paginatedMembers));

            A.CallTo(() => _mapper.Map<PaginatedList<MemberDTO>>(paginatedMembers))
                .Returns(mappedMembers);

            A.CallTo(() => _cacheService.GetOrSetCacheAsync(
                    A<string>._,
                    A<Func<Task<IDataResult<PaginatedList<MemberDTO>>>>>._))
                .ReturnsLazily(async call =>
                {
                    var factory = call.GetArgument<Func<Task<IDataResult<PaginatedList<MemberDTO>>>>>(1);
                    return await factory();
                });

            // Act
            var result = await _manager.RetrieveAllMembers(pageIndex, pageSize);

            // Assert
            Assert.Equal(ResponseCode_Success, result.ResponseCode);
            Assert.Single(result.Data.Items);
        }

        [Fact]
        public async Task CreateMemberAndAssignRolesAsync_ValidRequest_CreatesMember()
        {
            // Arrange
            var request = new MemberRequest
            {
                Email = "test@example.com",
                Password = "password",
                NickName = "testuser",
                ProfilePicture = A.Fake<IFormFile>()
            };
            var roles = new List<string> { "Member" };
            var httpContext = A.Fake<HttpContext>();
            var storageService = A.Fake<IStorage>();
            var expectedPath = "uploads/testuser/profile.jpg";

            // Mock storage operations - adjust to match your actual return type
            A.CallTo(() => _storageFactory.CreateStorage(StorageType.Local))
                .Returns(storageService);

            // Given storage service returns a tuple (string fileName, string pathOrContainerName)
            // First create the expected return value
            var uploadResult = new SuccessDataResult<(string fileName, string pathOrContainerName)>(
                ("profile.jpg", expectedPath));

            // Then set up the mock correctly
            A.CallTo(() => storageService.SingleUploadAsync(
                    A<string>._,
                    A<IFormFile>._,
                    A<HttpContext>._))
                .Returns(Task.FromResult<IDataResult<(string fileName, string pathOrContainerName)>>(uploadResult));

            // Mock user creation
            A.CallTo(() => _userManager.CreateAsync(
                    A<Member>.That.Matches(m =>
                        m.Email == request.Email &&
                        m.UserName == request.NickName),
                    request.Password))
                .Returns(Task.FromResult(IdentityResult.Success));

            // Mock role assignments
            foreach (var role in roles)
            {
                A.CallTo(() => _userManager.AddToRoleAsync(
                        A<Member>._,
                        role))
                    .Returns(Task.FromResult(IdentityResult.Success));
            }

            // Act
            var result = await _manager.CreateMemberAndAssignRolesAsync(request, roles, httpContext);

            // Assert
            Assert.Equal(ResponseCode_Success, result.ResponseCode);

            // Verify storage interaction
            A.CallTo(() => storageService.SingleUploadAsync(
                    A<string>.That.Contains(request.NickName),
                    request.ProfilePicture,
                    httpContext))
                .MustHaveHappenedOnceExactly();

            // Verify user creation
            A.CallTo(() => _userManager.CreateAsync(
                    A<Member>.That.Matches(m =>
                        m.ProfilePicture == expectedPath),
                    request.Password))
                .MustHaveHappenedOnceExactly();

            // Verify transaction completion
            A.CallTo(() => _unitOfWork.CommitAsync())
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task UpdateMemberAsync_ValidRequest_UpdatesMember()
        {
            // Arrange
            var memberId = "mem-123";
            var request = new MemberUpdateRequest
            {
                Email = "updated@example.com",
                NickName = "updateduser"
            };
            var httpContext = A.Fake<HttpContext>();
            var member = new Member { Id = memberId };

            A.CallTo(() => _userManager.FindByIdAsync(memberId))
                .Returns(Task.FromResult(member));

            A.CallTo(() => _userManager.UpdateAsync(member))
                .Returns(Task.FromResult(IdentityResult.Success));

            // Act
            var result = await _manager.UpdateMemberAsync(memberId, request, httpContext);

            // Assert
            Assert.Equal(ResponseCode_Success, result.ResponseCode);
            A.CallTo(() => _unitOfWork.CommitAsync()).MustHaveHappened();
        }

        [Fact]
        public async Task SoftDeleteMemberAsync_ValidId_DeletesMember()
        {
            // Arrange
            var memberId = "mem-123";
            var member = new Member { Id = memberId };

            A.CallTo(() => _memberDal.GetById(memberId))
                .Returns(Task.FromResult(member));

            // Act
            var result = await _manager.SoftDeleteMemberAsync(memberId);

            // Assert
            Assert.Equal(ResponseCode_Success, result.ResponseCode);
            A.CallTo(() => _memberDal.SoftDelete(memberId)).MustHaveHappened();
            A.CallTo(() => _unitOfWork.Commit()).MustHaveHappened();
        }
    }
}
