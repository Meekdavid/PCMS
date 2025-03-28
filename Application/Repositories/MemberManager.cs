using Application.Interfaces.Business;
using Application.Interfaces.Database;
using Application.Interfaces.General;
using AutoMapper;
using Common.ConfigurationSettings;
using Common.DTOs.Requests;
using Common.DTOs.Responses;
using Common.Models;
using Common.Pagination;
using Core.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Persistence.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Common.Literals.StringLiterals;
using Persistence.Enums;
using Common.Services;
using AutoMapper.Execution;
using Member = Persistence.DBModels.Member;

namespace Application.Repositories
{
    public class MemberManager :  IUserManager
    {
        private UserManager<Member> _memberManager { get; set; }
        private readonly ILogger<MemberManager> _logger;
        private readonly Common.Services.ITokenHandler _tokenHandler;
        private readonly ICacheService _cacheService;
        private readonly IEmailServiceCustom _emailHandler;
        private readonly IMemberRepository _memberDal;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IStorageFactory _storageFactory;
        private readonly int _refreshTokenExpiryDays = ConfigSettings.ApplicationSetting.RefreshTokenExpiryDays;
        public MemberManager(UserManager<Member> memberManager,
            ILogger<MemberManager> logger,
            Common.Services.ITokenHandler tokenHandler,
            IMemberRepository memberDal,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IStorageFactory storage,
            IEmailServiceCustom emailHandler,
            ICacheService cacheService)
        {
            _logger = logger;
            _memberManager = memberManager;
            _tokenHandler = tokenHandler;
            _memberDal = memberDal;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _storageFactory = storage;
            _emailHandler = emailHandler;
            _cacheService = cacheService;
        }
        public async Task<Core.Results.IResult> ChangeRefreshToken(MemberChangeRefreshTokenRequest request)
        {
            // Log the start of the refresh token update process
            _logger.LogInformation($"Changing refresh token for MemberId: {request.MemberId}");

            // Retrieve the member from the database using the MemberId
            var member = await _memberDal.GetById(request.MemberId);
            if (member == null)
            {
                // Log and return an error result if the member is not found
                _logger.LogInformation($"Member not found for MemberId: {request.MemberId}");
                return new ErrorDataResult<Token>(null, ResponseCode_MemberNotFound, ResponseMessage_MemberNotFound);
            }

            // Update the member's refresh token and its expiration date for validation purposes
            member.RefreshToken = request.RefreshToken;
            member.RefreshTokenEndDate = request.RefreshTokenEndDate;

            // Save the changes to the database
            await _memberDal.Update(member);
            await _unitOfWork.SaveChangesAsync(); // Save transaction changes
            await _unitOfWork.CommitAsync();     // Commit the transaction

            // Log success after the refresh token is updated
            _logger.LogInformation($"Refresh token updated successfully for MemberId: {request.MemberId}");

            // Return a success result
            return new SuccessResult("Refresh token updated successfully");
        }

        public async Task<IDataResult<string>> ChangeMemberPasswordAsync(ChangePasswordRequest req, string MemberId)
        {
            // Log the start of the password change process
            _logger.LogInformation($"Changing password for member ID: {MemberId}");

            // Attempt to find the member in the database using their ID
            var member = await _memberManager.FindByIdAsync(MemberId);

            if (member == null)
            {
                // Log and return an error if the member is not found
                _logger.LogInformation($"Member not found with ID: {MemberId}");
                return new ErrorDataResult<string>("", ResponseCode_MemberNotFound, ResponseMessage_MemberNotFound);
            }

            // Validate the provided current password against the stored password
            var res = await _memberManager.CheckPasswordAsync(member, req.Password);

            if (!res)
            {
                // Log and return an error if the current password is incorrect
                _logger.LogInformation($"Incorrect password for member ID: {MemberId}");
                return new ErrorDataResult<string>("", ResponseCode_WrongPassword, ResponseMessage_WrongPassword);
            }

            // Attempt to remove the existing password from the member's account
            var removePasswordResult = await _memberManager.RemovePasswordAsync(member);

            if (!removePasswordResult.Succeeded)
            {
                // Log and return an error if removing the existing password fails
                _logger.LogInformation($"Failed to remove password for member ID: {MemberId}");
                return new ErrorDataResult<string>("", ResponseCode_UnableToRemovePassword, ResponseMessage_UnableToRemovePassword);
            }

            // Attempt to add the new password to the member's account
            var addPasswordResult = await _memberManager.AddPasswordAsync(member, req.NewPassword);

            if (!addPasswordResult.Succeeded)
            {
                // Log and return an error if adding the new password fails
                _logger.LogInformation($"Failed to add new password for member ID: {MemberId}");
                return new ErrorDataResult<string>("", ResponseCode_FailedToAddNewPassword, ResponseMessage_FailedToAddNewPassword);
            }

            // Log success and return a success result once the password is updated
            _logger.LogInformation($"Password changed successfully for member ID: {MemberId}");
            return new SuccessDataResult<string>("");
        }
        public async Task<IDataResult<Token>> CreateAccessToken(Member member)
        {
            // Log the initiation of access token creation
            _logger.LogInformation($"Creating access token for MemberId: {member.Id}");

            // Retrieve the list of roles assigned to the member
            var rolesResult = await _memberManager.GetRolesAsync(member);

            // Generate an access token for the member, including their roles
            Token token = await _tokenHandler.CreateAccessTokenAsync(member, rolesResult.ToList());

            // Prepare a request to update the refresh token and its expiration date
            var changeRefreshTokenRequest = new MemberChangeRefreshTokenRequest
            {
                MemberId = member.Id,
                RefreshToken = token.RefreshToken,
                RefreshTokenEndDate = token.Expiration.AddDays(_refreshTokenExpiryDays) // Extend refresh token expiration
            };

            // Update the member's refresh token in the database
            await this.ChangeRefreshToken(changeRefreshTokenRequest);

            // Update the member details in the database (if necessary)
            await _memberDal.Update(member);

            // Persist the changes with transaction management
            await _unitOfWork.SaveChangesAsync(); // Save changes to the database
            await _unitOfWork.CommitAsync();      // Commit the transaction

            // Return the generated token as a success result
            return new SuccessDataResult<Token>(token);
        }

        public async Task<IDataResult<Token>> CreateAccessToken(string MemberId)
        {
            // Log the start of the access token creation process
            _logger.LogInformation($"Creating access token for MemberId: {MemberId}");

            // Retrieve the member from the database by their MemberId
            Member member = await _memberDal.GetById(MemberId);
            if (member == null)
            {
                // Log and return an error if the member is not found
                _logger.LogInformation($"Member not found for MemberId: {MemberId}");
                return new ErrorDataResult<Token>(null, ResponseCode_MemberNotFound, ResponseMessage_MemberNotFound);
            }

            // Retrieve the roles associated with the member
            var rolesResult = await _memberManager.GetRolesAsync(member);

            // Generate a new access token for the member based on their roles
            Token token = await _tokenHandler.CreateAccessTokenAsync(member, rolesResult.ToList());

            // Prepare a request to update the refresh token and its expiration date
            var changeRefreshTokenRequest = new MemberChangeRefreshTokenRequest
            {
                MemberId = member.Id,
                RefreshToken = token.RefreshToken,
                RefreshTokenEndDate = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays) // Refresh token expiry in days
            };

            // Update the member's refresh token in the database
            await this.ChangeRefreshToken(changeRefreshTokenRequest);

            // Return the generated token wrapped in a success result
            return new SuccessDataResult<Token>(token);
        }


        public async Task<IDataResult<string>> CreateMemberAndAssignRolesAsync(MemberRequest memberRegisterRequest, List<string> roles, HttpContext httpContext)
        {
            // Save user's profile picture to the specified storage and retrieve its path
            var storageInstance = _storageFactory.CreateStorage(StorageType.Local);
            string pathNewName = $"Uploads/User/{memberRegisterRequest.NickName}";
            var profilePicture = await storageInstance.SingleUploadAsync(pathNewName, memberRegisterRequest.ProfilePicture, httpContext);

            // Create a new member instance and populate its properties with request data
            var member = new Member
            {
                FirstName = memberRegisterRequest.FirstName,
                LastName = memberRegisterRequest.LastName,
                NickName = memberRegisterRequest.NickName,
                UserName = memberRegisterRequest.NickName,

                // Set the path of the uploaded profile picture
                ProfilePicture = profilePicture.Data.pathOrContainerName,

                DateOfBirth = memberRegisterRequest.DateOfBirth,
                NationalIdentificationNumber = memberRegisterRequest.NationalIdentificationNumber,
                Address = memberRegisterRequest.Address,

                PhoneNumber = memberRegisterRequest.PhoneNumber, // IdentityUser property
                Email = memberRegisterRequest.Email,            // IdentityUser property

                BankAccountNumber = memberRegisterRequest.BankAccountNumber,
                BankName = memberRegisterRequest.BankName,
                EmployerId = memberRegisterRequest.EmployerId,
                MembershipType = memberRegisterRequest.MembershipType
            };

            _logger.LogInformation($"Transaction started for creating member with Email: {memberRegisterRequest.Email}");

            // Attempt to create the member in the database
            var result = await _memberManager.CreateAsync(member, memberRegisterRequest.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation($"Member created successfully with Email: {memberRegisterRequest.Email}. Assigning roles...");

                // Assign each role from the roles list to the newly created member
                foreach (var role in roles)
                {
                    var res = await _memberManager.AddToRoleAsync(member, role);
                    if (!res.Succeeded)
                    {
                        // Log and rollback if any role assignment fails
                        _logger.LogInformation($"Failed to assign role '{role}' to member with Email: {member.Email}");
                        await _unitOfWork.Rollback();
                        return new ErrorDataResult<string>(ResponseCode_RoleAssignmentFailed, ResponseMessage_RoleAssignmentFailed);
                    }
                    _logger.LogInformation($"Role '{role}' assigned successfully to user with Email: {member.Email}");
                }

                // Save changes and commit the transaction if all roles were successfully assigned
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
                _logger.LogInformation($"Transaction committed and changes saved for member with Email: {member.Email}");

                // Return the member ID as the result
                return new SuccessDataResult<string>(member.Id);
            }
            else
            {
                // Delete the uploaded profile picture if member creation fails
                await storageInstance.DeleteAsync(profilePicture.Data.pathOrContainerName, "");
                _logger.LogInformation($"Member creation failed for Email: {member.Email}");

                // Rollback the transaction and return an error with details
                await _unitOfWork.Rollback();
                return new ErrorDataResult<string>(JsonConvert.SerializeObject(result.Errors), ResponseCode_MemberCreationFailed, ResponseMessage_MemberCreationFailed);
            }
        }


        public async Task<IDataResult<PaginatedList<MemberDTO>>> RetrieveAllMembers(int pageIndex, int pageSize)
        {
            _logger.LogInformation($"Received request to retrieve all members with pageSize {pageSize} and pageIndex {pageIndex}");

            // Generate a unique cache key using the page index and size to store/retrieve cached data
            string cacheKeyword = $"RetrieveAllMembers{pageIndex}{pageSize}";

            // Retrieve member from cache, if member does not exist in cache, reteieve from the database and save to cache
            return await _cacheService.GetOrSetCacheAsync(
                cacheKeyword,
                async () =>
                {
                    // Fetch members from the data source with pagination and include related 'Accounts' navigation property
                    var members = await _memberDal.GetAll(pageIndex, pageSize, null, includeProperties: m => m.Accounts);

                    // Map the retrieved members to a DTO format for response
                    var mappedMembers = _mapper.Map<PaginatedList<MemberDTO>>(members);

                    // Return the successfully retrieved and mapped members wrapped in a success result
                    return new SuccessDataResult<PaginatedList<MemberDTO>>(mappedMembers);
                }
            );
        }


        public async Task<IDataResult<MemberDTO>> RetrieveMemberById(string memberId)
        {
            // Log the request details, capturing the requested member ID
            _logger.LogInformation($"Received request to retrieve member with ID: {memberId}");

            // Generate a unique cache key using the member ID to cache individual member details
            string cacheKeyword = $"RetrieveMemberById_{memberId}";

            // Retrieve member from cache, if member does not exist in cache, reteieve from the database and save to cache
            var member = await _cacheService.GetOrSetCacheAsync<MemberDTO>(
                cacheKeyword,
                async () =>
                {
                    // Fetch the member from the data source based on the provided member ID
                    var member = await _memberDal.Get(m => m.Id == memberId); // Correct property is 'Id'

                    // Map the retrieved member entity to a DTO format
                    var mappedMember = _mapper.Map<MemberDTO>(member);

                    return mappedMember;
                }
            );

            // If the member is not found, return a failure response
            if (member == null)
            {
                _logger.LogWarning($"Member with ID {memberId} not found.");
                return new ErrorDataResult<MemberDTO>(null, ResponseCode_MemberNotFound, ResponseMessage_MemberNotFound);
            }            

            // Return the successfully retrieved and mapped member wrapped in a success result
            return new SuccessDataResult<MemberDTO>(member);
        }



        public async Task<IDataResult<PaginatedList<MemberDTO>>> RetrieveMemberByType(int pageIndex, int pageSize, MembershipType type)
        {
            // Log the request details, including page index, page size, and membership type
            _logger.LogInformation($"Received request to retrieve all members with pageSize {pageSize}, pageIndex {pageIndex}, and membershipType {type}");

            // Generate a unique cache key using page index, size, and membership type to differentiate cached data
            string cacheKeyword = $"RetrieveMemberByType{pageIndex}{pageSize}{type}";

            // Retrieve member from cache, if member does not exist in cache, reteieve from the database and save to cache
            return await _cacheService.GetOrSetCacheAsync(
                cacheKeyword,
                async () =>
                {
                    // Fetch members from the data source with pagination, filtering by the specified membership type,
                    // and include related 'Accounts' navigation property
                    var members = await _memberDal.GetAll(pageIndex, pageSize, m => m.MembershipType == type, includeProperties: m => m.Accounts);

                    // Map the retrieved members to a DTO format for structured API response
                    var mappedMembers = _mapper.Map<PaginatedList<MemberDTO>>(members);

                    // Wrap the mapped members in a success result and return
                    return new SuccessDataResult<PaginatedList<MemberDTO>>(mappedMembers);
                }
            );

        }

        public async Task<IDataResult<Token>> SignInAsync(string email, string password)
        {
            // Log the beginning of the sign-in process for the provided email
            _logger.LogInformation($"Attempting sign-in for Email: {email}");

            // Find the member by their email address
            var member = await _memberManager.FindByEmailAsync(email);
            if (member != null) // Check if the member exists
            {
                // Validate the member's password
                var login = await _memberManager.CheckPasswordAsync(member, password);

                if (login) // Check if the password is correct
                {
                    // If the email is not confirmed, log it and return an error
                    if (!member.EmailConfirmed)
                    {
                        _logger.LogInformation($"Email not confirmed for Member: {member.Id}");
                        return new ErrorDataResult<Token>(null, ResponseCode_MemberEmailNotConfirmed, ResponseMessage_MemberEmailNotConfirmed);
                    }

                    // Create an access token for the authenticated member
                    var tokenResult = await this.CreateAccessToken(member);
                    _logger.LogInformation($"Sign-in successful, token created for UserId: {member.Id}");

                    // Return the generated token
                    return tokenResult;
                }
                else
                {
                    // Log and return an error for incorrect password
                    _logger.LogInformation($"Login failed for Email: {email} {JsonConvert.SerializeObject(member ?? new Member())}");
                    return new ErrorDataResult<Token>(null, ResponseCode_LoginFailed, "Username or Password Incorrect");
                }
            }
            else
            {
                // Log and return an error if the account is not found
                _logger.LogInformation($"Login failed for Email: {email} {JsonConvert.SerializeObject(member ?? new Member())}");
                return new ErrorDataResult<Token>(null, ResponseCode_LoginFailed, "Account not Found");
            }
        }

        public async Task<IDataResult<string>> UpdateMemberAsync(string memberId, MemberUpdateRequest memberUpdateRequest, HttpContext httpContext)
        {
            // Find the existing member
            var member = await _memberManager.FindByIdAsync(memberId);
            if (member == null)
            {
                // Log and return an error if the member is not found
                _logger.LogInformation($"Member not found with ID: {memberId}");
                return new ErrorDataResult<string>("", ResponseCode_MemberNotFound, ResponseMessage_MemberNotFound);
            }

            // If profile picture is provided, update it
            if (memberUpdateRequest.ProfilePicture != null)
            {
                var storageInstance = _storageFactory.CreateStorage(StorageType.Local);
                string pathNewName = $"Uploads/User/{memberUpdateRequest.NickName}";
                var profilePicture = await storageInstance.SingleUploadAsync(pathNewName, memberUpdateRequest.ProfilePicture, httpContext);
                member.ProfilePicture = profilePicture.Data.pathOrContainerName;
            }

            // Update other member details
            member.FirstName = memberUpdateRequest.FirstName;
            member.LastName = memberUpdateRequest.LastName;
            member.NickName = memberUpdateRequest.NickName;
            member.DateOfBirth = memberUpdateRequest.DateOfBirth;
            member.NationalIdentificationNumber = memberUpdateRequest.NationalIdentificationNumber;
            member.Address = memberUpdateRequest.Address;
            member.PhoneNumber = memberUpdateRequest.PhoneNumber;
            member.Email = memberUpdateRequest.Email;
            member.BankAccountNumber = memberUpdateRequest.BankAccountNumber;
            member.BankName = memberUpdateRequest.BankName;
            member.EmployerId = memberUpdateRequest.EmployerId;
            member.MembershipType = memberUpdateRequest.MembershipType;

            _logger.LogInformation($"Transaction started for updating member with ID: {memberId}");

            // Attempt to update the member
            var result = await _memberManager.UpdateAsync(member);

            if (result.Succeeded)
            {
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
                _logger.LogInformation($"Member with ID {memberId} updated successfully.");

                return new SuccessDataResult<string>(member.Id);
            }
            else
            {
                await _unitOfWork.Rollback();
                _logger.LogInformation($"Failed to update member with ID {memberId}.");
                return new ErrorDataResult<string>(JsonConvert.SerializeObject(result.Errors), "Member update failed");
            }
        }

        public async Task<Core.Results.IResult> SoftDeleteMemberAsync(string memberId)
        {
            _logger.LogInformation($"About soft deleting member with ID: {memberId}");

            var member = await _memberDal.GetById(memberId);
            if (member == null)
            {
                _logger.LogInformation($"Member not found for MemberId: {memberId}");
                return new ErrorDataResult<Token>(null, ResponseCode_MemberNotFound, ResponseMessage_MemberNotFound);
            }

            await _memberDal.SoftDelete(memberId);
            await _unitOfWork.SaveChanges();
            await _unitOfWork.Commit();

            _logger.LogInformation($"Member deleted successfully for memberId: {memberId}");

            return new SuccessResult("Member deleted successfully");
        }
    }
}
