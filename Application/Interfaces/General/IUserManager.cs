using Common.DTOs;
using Common.DTOs.Requests;
using Common.DTOs.Responses;
using Common.Models;
using Common.Pagination;
using Core.Results;
using Microsoft.AspNetCore.Http;
using Persistence.DBModels;
using Persistence.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.General
{
    /// <summary>
    /// Defines an interface for managing member-related operations, including authentication, registration, password management, and member retrieval.
    /// </summary>
    public interface IUserManager
    {
        /// <summary>
        /// Asynchronously creates an access token for a given member.
        /// </summary>
        /// <param name="member">The member for whom to create the access token.</param>
        /// <returns>A task representing the asynchronous operation, returning a data result containing the generated token.</returns>
        Task<IDataResult<Token>> CreateAccessToken(Member member);

        /// <summary>
        /// Asynchronously creates a new member and assigns specified roles.
        /// </summary>
        /// <param name="memberRegisterRequest">The member registration request containing member details.</param>
        /// <param name="roles">The list of roles to assign to the new member.</param>
        /// <param name="httpContext">The HttpContext for use during member creation operations.</param>
        /// <returns>A task representing the asynchronous operation, returning a data result containing a message indicating success or failure.</returns>
        Task<IDataResult<string>> CreateMemberAndAssignRolesAsync(MemberRequest memberRegisterRequest, List<string> roles, HttpContext httpContext);

        /// <summary>
        /// Asynchronously signs in a member with the provided email and password.
        /// </summary>
        /// <param name="email">The member's email address.</param>
        /// <param name="password">The member's password.</param>
        /// <returns>A task representing the asynchronous operation, returning a data result containing the authentication token.</returns>
        Task<IDataResult<Token>> SignInAsync(string email, string password);

        /// <summary>
        /// Asynchronously changes a member's password.
        /// </summary>
        /// <param name="req">The change password request containing necessary information.</param>
        /// <param name="MemberId">The ID of the member whose password is being changed.</param>
        /// <returns>A task representing the asynchronous operation, returning a data result containing a message indicating success or failure.</returns>
        Task<IDataResult<string>> ChangeMemberPasswordAsync(ChangePasswordRequest req, string MemberId);

        /// <summary>
        /// Asynchronously creates an access token for a given member ID.
        /// </summary>
        /// <param name="MemberId">The ID of the member for whom to create the access token.</param>
        /// <returns>A task representing the asynchronous operation, returning a data result containing the generated token.</returns>
        Task<IDataResult<Token>> CreateAccessToken(string MemberId);

        /// <summary>
        /// Asynchronously changes a member's refresh token.
        /// </summary>
        /// <param name="request">The request containing the new refresh token.</param>
        /// <returns>A task representing the asynchronous operation, returning a result indicating success or failure.</returns>
        Task<Core.Results.IResult> ChangeRefreshToken(MemberChangeRefreshTokenRequest request);

        /// <summary>
        /// Asynchronously performs a soft delete on a member, marking it as deleted without physically removing it from the database.
        /// </summary>
        /// <param name="memberId">The ID of the member to soft delete.</param>
        /// <returns>A task representing the asynchronous operation, returning a result indicating success or failure.</returns>
        Task<Core.Results.IResult> SoftDeleteMemberAsync(string memberId);

        /// <summary>
        /// Asynchronously retrieves a member by their ID.
        /// </summary>
        /// <param name="Id">The ID of the member to retrieve.</param>
        /// <returns>A task representing the asynchronous operation, returning a data result containing the member's information.</returns>
        Task<IDataResult<MemberDTO>> RetrieveMemberById(string Id);

        /// <summary>
        /// Asynchronously retrieves a paginated list of members by membership type.
        /// </summary>
        /// <param name="pageIndex">The page index.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="type">The membership type to filter by.</param>
        /// <returns>A task representing the asynchronous operation, returning a data result containing a paginated list of member information.</returns>
        Task<IDataResult<PaginatedList<MemberDTO>>> RetrieveMemberByType(int pageIndex, int pageSize, MembershipType type);

        /// <summary>
        /// Asynchronously retrieves a paginated list of all members.
        /// </summary>
        /// <param name="pageIndex">The page index.</param>
        /// <param name="pageSize">The page size.</param>
        /// <returns>A task representing the asynchronous operation, returning a data result containing a paginated list of member information.</returns>
        Task<IDataResult<PaginatedList<MemberDTO>>> RetrieveAllMembers(int pageIndex, int pageSize);

        /// <summary>
        /// Asynchronously updates a member with the provided information.
        /// </summary>
        /// <param name="memberId">The ID of the member to update.</param>
        /// <param name="memberUpdateRequest">The updated member information.</param>
        /// <param name="httpContext">The HTTP context for use during the update operation.</param>
        /// <returns>A task representing the asynchronous operation, returning a data result containing a message indicating success or failure.</returns>
        Task<IDataResult<string>> UpdateMemberAsync(string memberId, MemberUpdateRequest memberUpdateRequest, HttpContext httpContext);
    }
}
