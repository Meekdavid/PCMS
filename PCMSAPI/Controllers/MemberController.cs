using Application.Interfaces.General;
using Common.DTOs.Requests;
using Common.DTOs.Responses;
using Common.Models;
using Common.Pagination;
using Core.Results;
using Microsoft.AspNetCore.Mvc;
using Persistence.Enums;

namespace API.Controllers
{
    [ApiController]
    [Route("api/member")]
    public class MemberController : ControllerBase
    {
        private readonly IUserManager _memberManager;

        public MemberController(IUserManager memberManager)
        {
            _memberManager = memberManager;
        }

        /// <summary>
        /// Registers a new member.
        /// </summary>
        /// <remarks>
        /// **Endpoint:** `POST /api/members/register`  
        ///  
        /// Registers a new member in the system.
        ///  
        /// **Response Codes:**  
        /// | Status Code | Response Code | Description |  
        /// |------------|--------------|-------------|  
        /// | 200        | 00           | Member registered successfully |  
        /// | 200        | 15           | Member role assignment failed |  
        /// | 200        | 16           | Member creation failed |  
        /// | 422        | 14           | Invalid input (e.g., missing fields) |  
        /// | 409        | 16           | Member already exists |  
        /// | 500        | 09           | Exception occurred, contact developer |  
        /// </remarks>
        [HttpPost("register")]
        [ProducesResponseType(typeof(SuccessDataResult<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status500InternalServerError)]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<IDataResult<string>>> Register([FromForm] MemberRequest newUser)
        {
            var result = await _memberManager.CreateMemberAndAssignRolesAsync(newUser, new List<string> { "Member" }, HttpContext);
            return Ok(result);
        }

        /// <summary>
        /// Updates a member records.
        /// </summary>
        /// <remarks>
        /// **Endpoint:** `PUT /api/members/update`  
        ///  
        /// Updates the details of an existing member.
        ///  
        /// **Response Codes:**  
        /// | Status Code | Response Code | Description |  
        /// |------------|--------------|-------------|  
        /// | 200        | 00           | Member updated successfully |  
        /// | 200        | 15           | Member role assignment failed |  
        /// | 200        | 19           | Member not found |  
        /// | 422        | 14           | Invalid input (e.g., missing fields) |  
        /// | 409        | 16           | Member already exists |  
        /// | 500        | 09           | Exception occurred, contact developer |  
        /// </remarks>
        /// <param name="memberId">Existing member unique Id.</param>
        [HttpPut("update")]
        [ProducesResponseType(typeof(SuccessDataResult<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status500InternalServerError)]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<IDataResult<string>>> Update([FromQuery] string memberId, [FromForm] MemberUpdateRequest updateRequest)
        {
            var result = await _memberManager.UpdateMemberAsync(memberId, updateRequest, HttpContext);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves details for a specific member.
        /// </summary>
        /// <remarks>
        /// **Endpoint:** `GET /api/members/memberId`  
        ///  
        /// Fetches details of a specific member by ID.
        ///  
        /// **Response Codes:**  
        /// | Status Code | Response Code | Description |  
        /// |------------|--------------|-------------|  
        /// | 200        | 00           | Member retrieved successfully | 
        /// | 200        | 19           | Member not found |  
        /// | 422        | 14           | Invalid input (e.g., missing fields) |  
        /// | 500        | 09           | Exception occurred, contact developer |  
        /// </remarks>
        /// <param name="memberId">Existing member unique Id.</param>
        [HttpGet]
        [ProducesResponseType(typeof(SuccessDataResult<MemberDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IDataResult<MemberDTO>>> RetrieveMemberById([FromQuery] string memberId)
        {
            var result = await _memberManager.RetrieveMemberById(memberId);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves all members.
        /// </summary>
        /// <remarks>
        /// **Endpoint:** `GET /api/members/memberId`  
        ///  
        /// Fetches details of all members with pagination.
        ///  
        /// **Response Codes:**  
        /// | Status Code | Response Code | Description |  
        /// |------------|--------------|-------------|  
        /// | 200        | 00           | Members retrieved successfully | 
        /// | 200        | 19           | Member not found |  
        /// | 422        | 14           | Invalid input (e.g., missing fields) |  
        /// | 500        | 09           | Exception occurred, contact developer |  
        /// </remarks>
        /// <param name="pageIndex">The page index (starting from 1).</param>
        /// <param name="pageSize">The number of records per page.</param>
        [HttpGet("all")]
        [ProducesResponseType(typeof(SuccessDataResult<PaginatedList<MemberDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IDataResult<PaginatedList<MemberDTO>>>> RetrieveAllMembers([FromQuery] int pageIndex, [FromQuery] int pageSize)
        {
            var result = await _memberManager.RetrieveAllMembers(pageIndex, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves members by membership type.
        /// </summary>
        /// <remarks>
        /// **Endpoint:** `GET /api/members`  
        ///  
        /// Fetches details of specific member types.
        ///  
        /// **Response Codes:**  
        /// | Status Code | Response Code | Description |  
        /// |------------|--------------|-------------|  
        /// | 200        | 00           | Members retrieved successfully | 
        /// | 200        | 19           | Member not found |  
        /// | 422        | 14           | Invalid input (e.g., missing fields) |  
        /// | 500        | 09           | Exception occurred, contact developer |  
        /// </remarks>
        /// <param name="pageIndex">The page index (starting from 1).</param>
        /// <param name="pageSize">The number of records per page.</param>
        /// <param name="memberType">The membership type. Value is 1 for Empoyer, 2 for Employee and 3 for Individual</param>
        [HttpGet("by-type")]
        [ProducesResponseType(typeof(SuccessDataResult<PaginatedList<MemberDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IDataResult<PaginatedList<MemberDTO>>>> RetrieveAllMembersbyMemberType([FromQuery] MembershipType memberType, [FromQuery] int pageIndex, [FromQuery] int pageSize)
        {
            var result = await _memberManager.RetrieveMemberByType(pageIndex, pageSize, memberType);
            return Ok(result);
        }

        /// <summary>
        /// Deletes a member records.
        /// </summary>
        /// <remarks>
        /// **Endpoint:** `DELETE /api/members/delete`  
        ///  
        /// Removes an existing member from the platform.
        ///  
        /// **Response Codes:**  
        /// | Status Code | Response Code | Description |  
        /// |------------|--------------|-------------|  
        /// | 200        | 00           | Member updated successfully |  
        /// | 200        | 15           | Member role assignment failed |  
        /// | 200        | 19           | Member not found |  
        /// | 422        | 14           | Invalid input (e.g., missing fields) |  
        /// | 409        | 16           | Member already exists |  
        /// | 500        | 09           | Exception occurred, contact developer |  
        /// </remarks>
        /// <param name="memberId">Existing member unique Id.</param>
        [HttpDelete]
        [ProducesResponseType(typeof(SuccessDataResult<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IDataResult<string>>> DeleteMember([FromQuery] string memberId)
        {
            var result = await _memberManager.SoftDeleteMemberAsync(memberId);
            return Ok(result);
        }
    }
}
