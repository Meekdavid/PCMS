using Application.Interfaces.Business;
using Application.Interfaces.Database;
using Application.Interfaces.General;
using AutoMapper;
using Common.DTOs.Requests;
using Common.DTOs.Responses;
using Common.Models;
using Common.Pagination;
using Common.Services;
using Core.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Persistence.DBModels;
using Persistence.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Common.Literals.StringLiterals;

namespace Application.Repositories
{
    public class EmployerManager : IEmployerManager
    {
        private readonly IEmployerRepository _employerDal;
        private readonly IMemberRepository _userDal;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<EmployerManager> _logger;
        private readonly ICacheService _cacheService;
        private readonly IMapper _mapper;
        private readonly IStorageFactory _storageFactory;
        private UserManager<Member> _userManager { get; set; }

        public EmployerManager(
            IEmployerRepository employerDal,
            IUnitOfWork unitOfWork,
            ILogger<EmployerManager> logger,
            ICacheService cacheService,
            IMapper mapper,
            IStorageFactory storageFactory,
            IMemberRepository userDal,
            UserManager<Member> userManager)
        {
            _employerDal = employerDal;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _cacheService = cacheService;
            _mapper = mapper;
            _storageFactory = storageFactory;
            _userDal = userDal;
            _userManager = userManager;
        }

        public async Task<IDataResult<string>> CreateEmployerAsync(EmployerRequest newEmployer, List<string> roles, HttpContext httpContext)
        {
            _logger.LogInformation($"Received request for creating employer: {newEmployer.CompanyName}");

            // Check if employer already exists
            var existingEmployer = (await _employerDal.GetAll(e => e.ContactEmail == newEmployer.ContactEmail))?.FirstOrDefault();
            if (existingEmployer != null)
            {
                _logger.LogWarning($"Employer already exists with email: {newEmployer.ContactEmail}");
                return new ErrorDataResult<string>(string.Empty, ResponseCode_EmployerAlreadyExists, ResponseMessage_EmployerAlreadyExists);
            }

            // Check if employer is a valid user of the platform
            var existingUser = await _userDal.GetByIdAsync(newEmployer.MemberId);
            if (existingUser == null)
            {
                _logger.LogWarning($"Member not found with ID: {newEmployer.MemberId}");
                return new ErrorDataResult<string>(string.Empty, ResponseCode_MemberNotFound, ResponseMessage_MemberNotFound);
            }

            // Map request to employer entity
            var employer = _mapper.Map<Employer>(newEmployer);
            employer.BankAccountNumber = existingUser.BankAccountNumber;
            employer.BankName = existingUser.BankName;

            // Handle logo upload if provided
            if (newEmployer.CompanyProfileImage != null)
            {
                var storageInstance = _storageFactory.CreateStorage(StorageType.Local);
                string pathNewName = $"Uploads/Employers/{employer.CompanyName}";
                var logoResult = await storageInstance.SingleUploadAsync(pathNewName, newEmployer.CompanyProfileImage, httpContext);
                employer.CompanyProfileImage = logoResult.Data.pathOrContainerName;
            }

            _logger.LogInformation($"Transaction started for creating employer: {employer.CompanyName}");

            // Assign roles
            var roleResult = await _userManager.AddToRolesAsync(existingUser, roles);
            if (!roleResult.Succeeded)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError($"Failed to assign roles to employer: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                return new ErrorDataResult<string>(JsonConvert.SerializeObject(roleResult.Errors), ResponseCode_RoleAssignmentFailed, ResponseMessage_RoleAssignmentFailed);
            }

            // Create employer
            await _employerDal.AddAsync(employer);

            // Commit transaction
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
            _logger.LogInformation($"Employer created successfully with ID: {employer.EmployerId}");

            return new SuccessDataResult<string>(employer.EmployerId);
        }

        public async Task<IDataResult<PaginatedList<EmployerDTO>>> RetrieveAllEmployersAsync(int pageIndex, int pageSize)
        {
            _logger.LogInformation($"Received request to retrieve all employers with pageSize {pageSize} and pageIndex {pageIndex}");

            // Generate cache key
            string cacheKeyword = $"RetrieveAllEmployers{pageIndex}{pageSize}";

            // Retrieve employers from cache if available; otherwise, fetch from DB and store in cache.
            return await _cacheService.GetOrSetCacheAsync(
                cacheKeyword,
                async () =>
                {
                    var employers = await _employerDal.GetAll(pageIndex, pageSize, null, includeProperties: e => e.Employees);
                    var mappedEmployers = _mapper.Map<PaginatedList<EmployerDTO>>(employers);
                    return new SuccessDataResult<PaginatedList<EmployerDTO>>(mappedEmployers);
                }
            );
        }

        public async Task<IDataResult<EmployerDTO>> RetrieveEmployerByIdAsync(string employerId)
        {
            _logger.LogInformation($"Received request to retrieve employer with ID: {employerId}");

            // Generate cache key
            string cacheKeyword = $"RetrieveEmployerById_{employerId}";

            // Retrieve employer from cache if available; otherwise, fetch from DB and store in cache.
            var employer = await _cacheService.GetOrSetCacheAsync<EmployerDTO>(
                cacheKeyword,
                async () =>
                {
                    var employer = await _employerDal.Get(e => e.EmployerId == employerId);
                    return _mapper.Map<EmployerDTO>(employer);
                }
            );

            if (employer == null)
            {
                _logger.LogWarning($"Employer with ID {employerId} not found.");
                return new ErrorDataResult<EmployerDTO>(null, ResponseCode_EmployerNotFound, ResponseMessage_EmployerNotFound);
            }

            return new SuccessDataResult<EmployerDTO>(employer);
        }

        public async Task<IDataResult<PaginatedList<EmployerDTO>>> RetrieveEmployersByStatusAsync(int pageIndex, int pageSize, bool isActive)
        {
            _logger.LogInformation($"Received request to retrieve employers with status {isActive}, pageSize {pageSize}, pageIndex {pageIndex}");

            // Generate cache key
            string cacheKeyword = $"RetrieveEmployersByStatus{isActive}{pageIndex}{pageSize}";

            // Retrieve employers from cache if available; otherwise, fetch from DB and store in cache.
            return await _cacheService.GetOrSetCacheAsync(
                cacheKeyword,
                async () =>
                {
                    var employers = await _employerDal.GetAll(pageIndex, pageSize, e => e.IsActive == isActive, includeProperties: e => e.Employees);
                    var mappedEmployers = _mapper.Map<PaginatedList<EmployerDTO>>(employers);
                    return new SuccessDataResult<PaginatedList<EmployerDTO>>(mappedEmployers);
                }
            );
        }

        public async Task<Core.Results.IResult> SoftDeleteEmployerAsync(string employerId)
        {
            _logger.LogInformation($"About to soft delete employer with ID: {employerId}");

            var employer = await _employerDal.GetById(employerId);
            if (employer == null)
            {
                _logger.LogInformation($"Employer not found for EmployerId: {employerId}");
                return new ErrorResult(ResponseCode_EmployerNotFound, ResponseMessage_EmployerNotFound);
            }

            await _employerDal.SoftDelete(employerId);
            await _unitOfWork.SaveChanges();
            await _unitOfWork.Commit();

            _logger.LogInformation($"Employer deleted successfully for employerId: {employerId}");

            return new SuccessResult("Employer deleted successfully");
        }

        public async Task<IDataResult<string>> UpdateEmployerAsync(string employerId, EmployerUpdateRequest updateRequest, HttpContext httpContext)
        {
            var employer = await _employerDal.GetByIdAsync(employerId);
            if (employer == null)
            {
                _logger.LogInformation($"Employer not found with ID: {employerId}");
                return new ErrorDataResult<string>("", ResponseCode_EmployerNotFound, ResponseMessage_EmployerNotFound);
            }

            // Update logo if provided
            if (updateRequest.CompanyProfileImage != null)
            {
                var storageInstance = _storageFactory.CreateStorage(StorageType.Local);
                string pathNewName = $"Uploads/Employers/{updateRequest.CompanyName}";
                var logoResult = await storageInstance.SingleUploadAsync(pathNewName, updateRequest.CompanyProfileImage, httpContext);
                employer.CompanyProfileImage = logoResult.Data.pathOrContainerName;
            }

            // Update other properties
            employer.CompanyName = updateRequest.CompanyName;
            employer.RegistrationNumber = updateRequest.RegistrationNumber;
            employer.Address = updateRequest.Address;
            employer.TaxIdentificationNumber = updateRequest.TaxIdentificationNumber;
            employer.Industry = updateRequest.Industry;
            employer.ContactEmail = updateRequest.ContactEmail;
            employer.ContactEmail = updateRequest.ContactEmail;
            employer.ContactPhone = updateRequest.ContactPhone;
            employer.WebsiteUrl = updateRequest.WebsiteUrl;
            employer.Country = updateRequest.Country;
            employer.State = updateRequest.State;
            employer.City = updateRequest.City;
            employer.NumberOfEmployees = updateRequest.NumberOfEmployees;
            employer.PensionContributionRate = updateRequest.PensionContributionRate;

            _logger.LogInformation($"Transaction started for updating employer with ID: {employerId}");

            await _employerDal.Update(employer);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
            _logger.LogInformation($"Employer with ID {employerId} updated successfully.");
            return new SuccessDataResult<string>(employer.EmployerId);
        }
    }
}
