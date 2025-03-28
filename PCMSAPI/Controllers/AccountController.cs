using Application.Interfaces.Business;
using Application.Repositories;
using Common.DTOs.Requests;
using Common.DTOs.Responses;
using Common.Models;
using Core.Results;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/accounts")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountManager _accountManager;

        public AccountController(IAccountManager accountManager)
        {
            _accountManager = accountManager;
        }

        /// <summary>
        /// Creates a new pension account.
        /// </summary>
        /// <remarks>
        /// **Endpoint:** `POST /api/accounts`  
        ///  
        /// Registers a new pension account for a member.
        ///  
        /// **Validation Requirements:**  
        /// - Member ID is required and must be valid  
        /// - Account type 1 is for individual contribution account, while 2 is for employer sponsored contribution account  
        ///  
        /// **Response Codes:**  
        /// | Status Code | Response Code | Description |  
        /// |------------|--------------|-------------|  
        /// | 200        | 00           | Account created successfully |  
        /// | 200        | 31           | Member not found |  
        /// | 422        | 14           | Invalid input (e.g., missing fields) |  
        /// | 500        | 09           | Exception occurred, contact developer |  
        /// </remarks>
        [HttpPost]
        [ProducesResponseType(typeof(SuccessDataResult<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IDataResult<string>>> CreateAccount([FromBody] NewAccountRequest request)
        {
            var result = await _accountManager.AddNewAccountAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves details of a specific account.
        /// </summary>
        /// <remarks>
        /// **Endpoint:** `GET /api/accounts/{accountId}`  
        ///  
        /// Fetches account details using the unique account ID.
        ///  
        /// **Response Codes:**  
        /// | Status Code | Response Code | Description |  
        /// |------------|--------------|-------------|  
        /// | 200        | 00           | Account retrieved successfully |  
        /// | 200        | 31           | Account not found |  
        /// | 422        | 14           | Invalid input (e.g., missing fields) |  
        /// | 500        | 09           | Exception occurred, contact developer |  
        /// </remarks>
        [HttpGet("{accountId}")]
        [ProducesResponseType(typeof(SuccessDataResult<AccountDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IDataResult<AccountDTO>>> GetById([FromRoute] string accountId)
        {
            var result = await _accountManager.RetrieveAccountByIdAsync(accountId);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves all accounts.
        /// </summary>
        /// <remarks>
        /// **Endpoint:** `GET /api/accounts`  
        ///  
        /// Fetches a list of all accounts.
        ///  
        /// **Response Codes:**  
        /// | Status Code | Response Code | Description |  
        /// |------------|--------------|-------------|  
        /// | 200        | 00           | Accounts retrieved successfully |  
        /// | 200        | 31           | No accounts found |  
        /// | 500        | 09           | Exception occurred, contact developer |  
        /// </remarks>
        [HttpGet]
        [ProducesResponseType(typeof(SuccessDataResult<List<AccountDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IDataResult<List<AccountDTO>>>> GetAll([FromQuery] int pageIndex, [FromQuery] int pageSize)
        {
            var result = await _accountManager.RetrieveAllAccountsAsync(pageIndex, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves accounts for a specific member.
        /// </summary>
        /// <remarks>
        /// **Endpoint:** `GET /api/accounts/member/{memberId}`  
        ///  
        /// Fetches all accounts associated with a specific member by their unique identifier.  
        ///  
        /// **Response Codes:**  
        /// | Status Code | Response Code | Description |  
        /// |------------|--------------|-------------|  
        /// | 200        | 00           | Accounts retrieved successfully | 
        /// | 200        | 31           | No accounts found for the member |  
        /// | 422        | 14           | Invalid input (e.g., missing or incorrect memberId) |  
        /// | 500        | 09           | Exception occurred, contact developer |  
        /// </remarks>
        /// <param name="memberId">Unique identifier for the member.</param>
        [HttpGet("member/{memberId}")]
        [ProducesResponseType(typeof(SuccessDataResult<List<AccountDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IDataResult<List<AccountDTO>>>> GetAccountsForMember(string memberId)
        {
            var result = await _accountManager.GetAccountsForSpecificMemberAsync(memberId);
            return Ok(result);
        }
    }

}
