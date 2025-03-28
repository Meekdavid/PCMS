using Application.Interfaces.Business;
using Application.Interfaces.General;
using Common.DTOs.Requests;
using Common.DTOs.Responses;
using Common.Models;
using Common.Pagination;
using Core.Results;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/employers")]
    public class EmployerController : ControllerBase
    {
        private readonly IEmployerManager _employerManager;

        public EmployerController(IEmployerManager employerManager)
        {
            _employerManager = employerManager;
        }

        /// <summary>
        /// Registers a new employer.
        /// </summary>
        /// <remarks>
        /// **Endpoint:** `POST /api/employers`  
        ///  
        /// Registers a new employer in the system.
        ///  
        /// **Validation Requirements:**  
        /// - Company name is required (minimum 2 characters)  
        /// - Registration number is required and must be valid  
        /// - Active status must be specified  
        ///  
        /// **Response Codes:**  
        /// | Status Code | Response Code | Description |  
        /// |------------|--------------|-------------|  
        /// | 200        | 00           | Employer registered successfully |  
        /// | 200        | 15           | Employer role assignment failed |  
        /// | 200        | 32           | Employer already exists |  
        /// | 200        | 16           | Employer creation failed |  
        /// | 422        | 14           | Invalid input (e.g., missing fields) |  
        /// | 500        | 09           | Exception occurred, contact developer |  
        /// </remarks>
        [HttpPost]
        [ProducesResponseType(typeof(SuccessDataResult<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status500InternalServerError)]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<IDataResult<string>>> Register([FromForm] EmployerRequest newEmployer)
        {
            var result = await _employerManager.CreateEmployerAsync(newEmployer, new List<string> { "Employer" }, HttpContext);
            return Ok(result);
        }

        /// <summary>
        /// Updates an employer's records.
        /// </summary>
        /// <remarks>
        /// **Endpoint:** `PUT /api/employers/{employerId}`  
        ///  
        /// Updates the details of an existing employer.
        ///  
        /// **Validation Requirements:**  
        /// - Company name is required (minimum 2 characters)  
        /// - Registration number is required and must be valid  
        /// - Active status must be specified  
        ///  
        /// **Response Codes:**  
        /// | Status Code | Response Code | Description |  
        /// |------------|--------------|-------------|  
        /// | 200        | 00           | Employer updated successfully |  
        /// | 200        | 15           | Employer role assignment failed |  
        /// | 200        | 31           | Employer not found |  
        /// | 422        | 14           | Invalid input (e.g., missing fields) |  
        /// | 409        | 16           | Employer already exists |  
        /// | 500        | 09           | Exception occurred, contact developer |  
        /// </remarks>
        /// <param name="employerId">Existing employer unique Id.</param>
        [HttpPut("{employerId}")]
        [ProducesResponseType(typeof(SuccessDataResult<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status500InternalServerError)]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<IDataResult<string>>> Update(string employerId, [FromForm] EmployerUpdateRequest updateRequest)
        {
            var result = await _employerManager.UpdateEmployerAsync(employerId, updateRequest, HttpContext);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves details for a specific employer.
        /// </summary>
        /// <remarks>
        /// **Endpoint:** `GET /api/employers/{employerId}`  
        ///  
        /// Fetches details of a specific employer by ID.
        ///  
        /// **Response Codes:**  
        /// | Status Code | Response Code | Description |  
        /// |------------|--------------|-------------|  
        /// | 200        | 00           | Employer retrieved successfully | 
        /// | 200        | 31           | Employer not found |  
        /// | 422        | 14           | Invalid input (e.g., missing fields) |  
        /// | 500        | 09           | Exception occurred, contact developer |  
        /// </remarks>
        /// <param name="employerId">Existing employer unique Id.</param>
        [HttpGet("{employerId}")]
        [ProducesResponseType(typeof(SuccessDataResult<EmployerDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IDataResult<EmployerDTO>>> GetById(string employerId)
        {
            var result = await _employerManager.RetrieveEmployerByIdAsync(employerId);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves all employers.
        /// </summary>
        /// <remarks>
        /// **Endpoint:** `GET /api/employers`  
        ///  
        /// Fetches details of all employers with pagination.
        ///  
        /// **Response Codes:**  
        /// | Status Code | Response Code | Description |  
        /// |------------|--------------|-------------|  
        /// | 200        | 00           | Employers retrieved successfully | 
        /// | 200        | 31           | No employers found |  
        /// | 422        | 14           | Invalid input (e.g., missing fields) |  
        /// | 500        | 09           | Exception occurred, contact developer |  
        /// </remarks>
        /// <param name="pageIndex">The page index (starting from 1).</param>
        /// <param name="pageSize">The number of records per page.</param>
        [HttpGet]
        [ProducesResponseType(typeof(SuccessDataResult<PaginatedList<EmployerDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IDataResult<PaginatedList<EmployerDTO>>>> GetAll([FromQuery] int pageIndex, [FromQuery] int pageSize)
        {
            var result = await _employerManager.RetrieveAllEmployersAsync(pageIndex, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves employers by active status.
        /// </summary>
        /// <remarks>
        /// **Endpoint:** `GET /api/employers/by-status`  
        ///  
        /// Fetches details of employers filtered by active status.
        ///  
        /// **Response Codes:**  
        /// | Status Code | Response Code | Description |  
        /// |------------|--------------|-------------|  
        /// | 200        | 00           | Employers retrieved successfully | 
        /// | 200        | 31           | No employers found |  
        /// | 422        | 14           | Invalid input (e.g., missing fields) |  
        /// | 500        | 09           | Exception occurred, contact developer |  
        /// </remarks>
        /// <param name="pageIndex">The page index (starting from 1).</param>
        /// <param name="pageSize">The number of records per page.</param>
        /// <param name="isActive">Filter by active status (true/false)</param>
        [HttpGet("by-status")]
        [ProducesResponseType(typeof(SuccessDataResult<PaginatedList<EmployerDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IDataResult<PaginatedList<EmployerDTO>>>> GetByStatus([FromQuery] bool isActive, [FromQuery] int pageIndex, [FromQuery] int pageSize)
        {
            var result = await _employerManager.RetrieveEmployersByStatusAsync(pageIndex, pageSize, isActive);
            return Ok(result);
        }

        /// <summary>
        /// Soft-deletes an employer.
        /// </summary>
        /// <remarks>
        /// **Endpoint:** `DELETE /api/employers/{employerId}`  
        ///  
        /// Marks an employer as deleted (soft delete) in the system.
        ///  
        /// **Response Codes:**  
        /// | Status Code | Response Code | Description |  
        /// |------------|--------------|-------------|  
        /// | 200        | 00           | Employer deleted successfully |  
        /// | 200        | 31           | Employer not found |  
        /// | 422        | 14           | Invalid input (e.g., missing fields) |  
        /// | 500        | 09           | Exception occurred, contact developer |  
        /// </remarks>
        /// <param name="employerId">Existing employer unique Id.</param>
        [HttpDelete("{employerId}")]
        [ProducesResponseType(typeof(SuccessDataResult<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ErrorDataResult<string>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IDataResult<string>>> Delete(string employerId)
        {
            var result = await _employerManager.SoftDeleteEmployerAsync(employerId);
            return Ok(result);
        }
    }
}
