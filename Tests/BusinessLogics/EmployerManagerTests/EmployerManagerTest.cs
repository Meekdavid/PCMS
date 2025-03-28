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

namespace Tests.BusinessLogics.EmployerManagerTests
{
    public class EmployerManagerTest
    {
        private readonly IEmployerRepository _employerDal;
        private readonly IMemberRepository _userDal;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<EmployerManager> _logger;
        private readonly ICacheService _cacheService;
        private readonly IMapper _mapper;
        private readonly IStorageFactory _storageFactory;
        private readonly UserManager<Member> _userManager;
        private readonly EmployerManager _manager;

        public EmployerManagerTest()
        {
            _employerDal = A.Fake<IEmployerRepository>();
            _userDal = A.Fake<IMemberRepository>();
            _unitOfWork = A.Fake<IUnitOfWork>();
            _logger = A.Fake<ILogger<EmployerManager>>();
            _cacheService = A.Fake<ICacheService>();
            _mapper = A.Fake<IMapper>();
            _storageFactory = A.Fake<IStorageFactory>();
            _userManager = A.Fake<UserManager<Member>>();

            _manager = new EmployerManager(
                _employerDal,
                _unitOfWork,
                _logger,
                _cacheService,
                _mapper,
                _storageFactory,
                _userDal,
                _userManager);
        }

        [Fact]
        public async Task CreateEmployerAsync_ValidRequest_CreatesEmployer()
        {
            // Arrange
            var request = new EmployerRequest
            {
                MemberId = "mem-123",
                CompanyName = "Test Company",
                ContactEmail = "test@company.com"
            };

            var roles = new List<string> { "Employer" };
            var httpContext = A.Fake<HttpContext>();
            var member = new Member { Id = "mem-123" };
            var identityResult = IdentityResult.Success;

            A.CallTo(() => _employerDal.GetAll(A<Expression<Func<Employer, bool>>>._))
                .Returns(Task.FromResult(new List<Employer>()));

            A.CallTo(() => _userDal.GetByIdAsync(request.MemberId))
                .Returns(Task.FromResult(member));

            A.CallTo(() => _storageFactory.CreateStorage(StorageType.Local))
                .Returns(A.Fake<IStorage>());

            A.CallTo(() => _userManager.AddToRolesAsync(member, roles))
                .Returns(Task.FromResult(identityResult));

            A.CallTo(() => _mapper.Map<Employer>(request))
                .Returns(new Employer { EmployerId = "emp-123" });

            // Act
            var result = await _manager.CreateEmployerAsync(request, roles, httpContext);

            // Assert
            Assert.Equal(ResponseCode_Success, result.ResponseCode);
            A.CallTo(() => _employerDal.AddAsync(A<Employer>._)).MustHaveHappened();
            A.CallTo(() => _unitOfWork.CommitAsync()).MustHaveHappened();
        }

        [Fact]
        public async Task RetrieveAllEmployersAsync_ReturnsPaginatedEmployers()
        {
            // Arrange
            var pageIndex = 1;
            var pageSize = 10;
            var employers = new List<Employer> { new Employer { EmployerId = "emp-123" } };
            var paginatedEmployers = new PaginatedList<Employer>(employers, 1, pageIndex, pageSize);
            var mappedEmployers = new PaginatedList<EmployerDTO>(
                new List<EmployerDTO> { new EmployerDTO { EmployerId = "emp-123" } },
                1, pageIndex, pageSize);

            A.CallTo(() => _employerDal.GetAll(
                    pageIndex,
                    pageSize,
                    null,
                    A<Expression<Func<Employer, object>>>._))
                .Returns(Task.FromResult(paginatedEmployers));

            A.CallTo(() => _mapper.Map<PaginatedList<EmployerDTO>>(paginatedEmployers))
                .Returns(mappedEmployers);

            A.CallTo(() => _cacheService.GetOrSetCacheAsync(
                    A<string>._,
                    A<Func<Task<IDataResult<PaginatedList<EmployerDTO>>>>>._))
                .ReturnsLazily(async call =>
                {
                    var factory = call.GetArgument<Func<Task<IDataResult<PaginatedList<EmployerDTO>>>>>(1);
                    return await factory();
                });

            // Act
            var result = await _manager.RetrieveAllEmployersAsync(pageIndex, pageSize);

            // Assert
            Assert.Equal(ResponseCode_Success, result.ResponseCode);
            Assert.Single(result.Data.Items);
        }

        [Fact]
        public async Task RetrieveEmployerByIdAsync_ValidId_ReturnsEmployer()
        {
            // Arrange
            var employerId = "emp-123";
            var employer = new Employer { EmployerId = employerId };
            var employerDto = new EmployerDTO { EmployerId = employerId };

            A.CallTo(() => _employerDal.Get(A<Expression<Func<Employer, bool>>>._))
                .Returns(employer);

            A.CallTo(() => _mapper.Map<EmployerDTO>(employer))
                .Returns(employerDto);

            A.CallTo(() => _cacheService.GetOrSetCacheAsync(
                    A<string>._,
                    A<Func<Task<EmployerDTO>>>._))
                .ReturnsLazily(async call => await call.GetArgument<Func<Task<EmployerDTO>>>(1)());

            // Act
            var result = await _manager.RetrieveEmployerByIdAsync(employerId);

            // Assert
            Assert.Equal(ResponseCode_Success, result.ResponseCode);
            Assert.Equal(employerId, result.Data.EmployerId);
        }

        [Fact]
        public async Task SoftDeleteEmployerAsync_ValidId_DeletesEmployer()
        {
            // Arrange
            var employerId = "emp-123";
            var employer = new Employer { EmployerId = employerId };

            A.CallTo(() => _employerDal.GetById(employerId))
                .Returns(Task.FromResult(employer));

            // Act
            var result = await _manager.SoftDeleteEmployerAsync(employerId);

            // Assert
            Assert.Equal(ResponseCode_Success, result.ResponseCode);
            A.CallTo(() => _employerDal.SoftDelete(employerId)).MustHaveHappened();
            A.CallTo(() => _unitOfWork.Commit()).MustHaveHappened();
        }

        [Fact]
        public async Task UpdateEmployerAsync_ValidRequest_UpdatesEmployer()
        {
            // Arrange
            var employerId = "emp-123";
            var request = new EmployerUpdateRequest
            {
                CompanyName = "Updated Company",
                ContactEmail = "updated@company.com"
            };
            var httpContext = A.Fake<HttpContext>();
            var employer = new Employer { EmployerId = employerId };

            A.CallTo(() => _employerDal.GetByIdAsync(employerId))
                .Returns(Task.FromResult(employer));

            A.CallTo(() => _storageFactory.CreateStorage(StorageType.Local))
                .Returns(A.Fake<IStorage>());

            // Act
            var result = await _manager.UpdateEmployerAsync(employerId, request, httpContext);

            // Assert
            Assert.Equal(ResponseCode_Success, result.ResponseCode);
            A.CallTo(() => _employerDal.Update(employer)).MustHaveHappened();
            A.CallTo(() => _unitOfWork.CommitAsync()).MustHaveHappened();
        }

        [Fact]
        public async Task CreateEmployerAsync_ExistingEmail_ReturnsError()
        {
            // Arrange
            var request = new EmployerRequest
            {
                MemberId = "mem-123",
                ContactEmail = "exists@company.com"
            };
            var httpContext = A.Fake<HttpContext>();
            var existingEmployer = new Employer { EmployerId = "emp-123" };

            A.CallTo(() => _employerDal.GetAll(A<Expression<Func<Employer, bool>>>._))
                .Returns(Task.FromResult(new List<Employer> { existingEmployer }));

            // Act
            var result = await _manager.CreateEmployerAsync(request, new List<string>(), httpContext);

            // Assert
            Assert.Equal(ResponseCode_EmployerAlreadyExists, result.ResponseCode);
        }

        [Fact]
        public async Task RetrieveEmployersByStatusAsync_ReturnsFilteredEmployers()
        {
            // Arrange
            var pageIndex = 1;
            var pageSize = 10;
            var isActive = true;
            var employers = new List<Employer> { new Employer { EmployerId = "emp-123", IsActive = true } };
            var paginatedEmployers = new PaginatedList<Employer>(employers, 1, pageIndex, pageSize);
            var mappedEmployers = new PaginatedList<EmployerDTO>(
                new List<EmployerDTO> { new EmployerDTO { EmployerId = "emp-123" } },
                1, pageIndex, pageSize);

            A.CallTo(() => _employerDal.GetAll(
                    pageIndex,
                    pageSize,
                    A<Expression<Func<Employer, bool>>>._,
                    A<Expression<Func<Employer, object>>>._))
                .Returns(Task.FromResult(paginatedEmployers));

            A.CallTo(() => _mapper.Map<PaginatedList<EmployerDTO>>(paginatedEmployers))
                .Returns(mappedEmployers);

            A.CallTo(() => _cacheService.GetOrSetCacheAsync(
                    A<string>._,
                    A<Func<Task<IDataResult<PaginatedList<EmployerDTO>>>>>._))
                .ReturnsLazily(async call =>
                {
                    var factory = call.GetArgument<Func<Task<IDataResult<PaginatedList<EmployerDTO>>>>>(1);
                    return await factory.Invoke();
                });

            // Act
            var result = await _manager.RetrieveEmployersByStatusAsync(pageIndex, pageSize, isActive);

            // Assert
            Assert.Equal(ResponseCode_Success, result.ResponseCode);
            Assert.Single(result.Data.Items);
        }
    }
}
