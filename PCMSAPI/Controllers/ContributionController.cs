using Application.Interfaces.Business;
using Common.DTOs.Requests;
using Common.DTOs.Responses;
using Common.Models;
using Core.Results;
using Microsoft.AspNetCore.Mvc;
using System.Transactions;
using static Common.Literals.StringLiterals;

namespace API.Controllers
{
    [ApiController]
    [Route("api/contributions")]
    public class ContributionController : ControllerBase
    {
        private readonly IContributionManager _contributionManager;

        public ContributionController(IContributionManager contributionManager)
        {
            _contributionManager = contributionManager;
        }

        /// <summary>
        /// Adds a new contribution (Monthly/Voluntary).
        /// </summary>
        /// <remarks>
        /// **Endpoint:** `POST /api/contributions`  
        ///  
        /// Adds a new contribution for a member.  
        ///  
        /// **Validation Requirements:**  
        /// - Contribution type must be valid  
        /// - Amount must be greater than zero  
        ///  
        /// **Response Codes:**  
        /// | Status Code | Response Code | Description |  
        /// |------------|--------------|-------------|  
        /// | 200        | 00           | Contribution added successfully |  
        /// | 422        | 14           | Invalid input (e.g., missing or invalid fields) |  
        /// | 500        | 09           | Exception occurred, contact developer |  
        /// </remarks>
        [HttpPost]
        [ProducesResponseType(typeof(SuccessDataResult<ContributionDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IDataResult<ContributionDTO>>> AddContribution([FromBody] ContributionRequest request)
        {
            var result = await _contributionManager.AddContributionAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves details of a specific contribution by ID.
        /// </summary>
        /// <remarks>
        /// **Endpoint:** `GET /api/contributions/{id}`  
        ///  
        /// Fetches details of a contribution by its unique identifier.  
        ///  
        /// **Response Codes:**  
        /// | Status Code | Response Code | Description |  
        /// |------------|--------------|-------------|  
        /// | 200        | 00           | Contribution retrieved successfully |  
        /// | 404        | 31           | Contribution not found |  
        /// | 500        | 09           | Exception occurred, contact developer |  
        /// </remarks>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(SuccessDataResult<ContributionDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IDataResult<ContributionDTO>>> GetContributionById(string id)
        {
            var result = await _contributionManager.GetContributionByIdAsync(id);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves all contributions for a member.
        /// </summary>
        /// <remarks>
        /// **Endpoint:** `GET /api/contributions/member/{memberId}`  
        ///  
        /// Fetches all contributions made by a specific member.  
        ///  
        /// **Response Codes:**  
        /// | Status Code | Response Code | Description |  
        /// |------------|--------------|-------------|  
        /// | 200        | 00           | Contributions retrieved successfully |  
        /// | 404        | 31           | Member contributions not found |  
        /// | 500        | 09           | Exception occurred, contact developer |  
        /// </remarks>
        [HttpGet("member/{memberId}")]
        [ProducesResponseType(typeof(SuccessDataResult<List<ContributionDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IDataResult<List<ContributionDTO>>>> GetContributionsByMember(string memberId)
        {
            var result = await _contributionManager.GetContributionsByMemberAsync(memberId);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves contribution summary for a member.
        /// </summary>
        /// <remarks>
        /// **Endpoint:** `GET /api/contributions/summary/{memberId}`  
        ///  
        /// Provides a summary of contributions made by a specific member.  
        ///  
        /// **Response Codes:**  
        /// | Status Code | Response Code | Description |  
        /// |------------|--------------|-------------|  
        /// | 200        | 00           | Contribution summary retrieved successfully |  
        /// | 404        | 31           | Contribution summary not found |  
        /// | 500        | 09           | Exception occurred, contact developer |  
        /// </remarks>
        [HttpGet("summary/{memberId}")]
        [ProducesResponseType(typeof(SuccessDataResult<ContributionDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IDataResult<ContributionDTO>>> GetContributionSummary(string memberId)
        {
            var result = await _contributionManager.GetContributionSummaryAsync(memberId);
            return Ok(result);
        }

        /// <summary>
        /// Generates contribution statements for members.
        /// </summary>
        /// <remarks>
        /// **Endpoint:** `GET /api/contributions/statements`  
        ///  
        /// Creates and retrieves detailed contribution statements based on query parameters.  
        ///  
        /// **Response Codes:**  
        /// | Status Code | Response Code | Description |  
        /// |------------|--------------|-------------|  
        /// | 200        | 00           | Statements generated successfully |  
        /// | 400        | 14           | Invalid input parameters |  
        /// | 500        | 09           | Exception occurred, contact developer |  
        /// </remarks>
        [HttpGet("statements")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GenerateStatements([FromQuery] StatementRequest request)
        {
            var result = await _contributionManager.GenerateStatementsAsync(request);

            if (result.ResponseCode == ResponseCode_Success)
            {
                return File(result.Data.Content, result.Data.ContentType, result.Data.FileName);
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Processes a pension withdrawal for an eligible member.
        /// </summary>
        /// <remarks>
        /// **Endpoint:** `POST /api/contributions/withdraw`
        /// 
        /// Processes a withdrawal from a member's pension account after verifying:
        /// - Member eligibility (IsEligibleForBenefits flag)
        /// - Sufficient account balance
        /// - Valid withdrawal amount
        /// 
        /// **Business Rules:**
        /// - Member must be marked as eligible for benefits
        /// - Withdrawal amount must be positive
        /// - Account must have sufficient balance
        /// - Updates account balance and creates transaction record
        /// 
        /// **Response Codes:**
        /// | Status Code | Response Code | Description |
        /// |------------|--------------|-------------|
        /// | 200        | 00           | Withdrawal processed successfully |
        /// | 400        | 31           | Insufficient funds for withdrawal |
        /// | 403        | 30           | Member not eligible for benefits |
        /// | 404        | 19/20        | Member or account not found |
        /// | 422        | 14           | Invalid input (e.g., amount ≤ 0) |
        /// | 500        | 09           | System error, contact developer |
        /// </remarks>
        /// <param name="request">Withdrawal request details</param>
        /// <returns>
        /// Returns withdrawal result with transaction details on success.
        /// Returns error response with appropriate status code on failure.
        /// </returns>
        [HttpPost("withdraw")]
        [ProducesResponseType(typeof(SuccessDataResult<WithdrawalResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IDataResult<WithdrawalResult>>> ProcessWithdrawal([FromBody] WithdrawalRequest request)
        {
            var result = await _contributionManager.ProcessWithdrawalAsync(request);

            if (result.ResponseCode == ResponseCode_Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Checks if a member is eligible for pension benefits
        /// </summary>
        /// <remarks>
        /// **Endpoint:** `GET /api/contributions/eligibility/{memberId}`
        /// 
        /// Verifies if a member meets all requirements to receive pension benefits.
        /// 
        /// **Validation Rules:**
        /// - Minimum contribution period (configurable)
        /// - Minimum age requirement (configurable)
        /// - Account in good standing
        /// 
        /// **Response Codes:**
        /// | Status Code | Response Code | Description |
        /// |------------|--------------|-------------|
        /// | 200        | 00           | Eligibility check completed |
        /// | 404        | 19           | Member not found |
        /// | 500        | 09           | System error |
        /// </remarks>
        /// <param name="memberId">Member unique identifier</param>
        [HttpGet("eligibility/{memberId}")]
        [ProducesResponseType(typeof(SuccessDataResult<EligibilityResultDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IDataResult<EligibilityResultDTO>>> CheckEligibility(string memberId)
        {
            var result = await _contributionManager.CheckEligibilityAsync(memberId);
            return Ok(result);
        }
    }

}
