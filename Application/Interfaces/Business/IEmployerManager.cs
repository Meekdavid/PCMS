using Common.DTOs.Requests;
using Common.DTOs.Responses;
using Common.Models;
using Common.Pagination;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Business
{
    /// <summary>
    /// Interface for managing employer-related operations.
    /// </summary>
    public interface IEmployerManager
    {
        /// <summary>
        /// Creates a new employer asynchronously.
        /// </summary>
        /// <param name="newEmployer">The new employer request data.</param>
        /// <param name="roles">List of roles associated with the employer.</param>
        /// <param name="httpContext">The HTTP context.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the ID of the created employer.</returns>
        Task<IDataResult<string>> CreateEmployerAsync(EmployerRequest newEmployer, List<string> roles, HttpContext httpContext);

        /// <summary>
        /// Retrieves all employers asynchronously with pagination.
        /// </summary>
        /// <param name="pageIndex">The index of the page to retrieve.</param>
        /// <param name="pageSize">The size of the page to retrieve.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a paginated list of employers.</returns>
        Task<IDataResult<PaginatedList<EmployerDTO>>> RetrieveAllEmployersAsync(int pageIndex, int pageSize);

        /// <summary>
        /// Retrieves an employer by ID asynchronously.
        /// </summary>
        /// <param name="employerId">The ID of the employer to retrieve.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the employer data.</returns>
        Task<IDataResult<EmployerDTO>> RetrieveEmployerByIdAsync(string employerId);

        /// <summary>
        /// Retrieves employers by their status asynchronously with pagination.
        /// </summary>
        /// <param name="pageIndex">The index of the page to retrieve.</param>
        /// <param name="pageSize">The size of the page to retrieve.</param>
        /// <param name="isActive">The status of the employers to retrieve.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a paginated list of employers.</returns>
        Task<IDataResult<PaginatedList<EmployerDTO>>> RetrieveEmployersByStatusAsync(int pageIndex, int pageSize, bool isActive);

        /// <summary>
        /// Updates an employer asynchronously.
        /// </summary>
        /// <param name="employerId">The ID of the employer to update.</param>
        /// <param name="updateRequest">The update request data.</param>
        /// <param name="httpContext">The HTTP context.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the ID of the updated employer.</returns>
        Task<IDataResult<string>> UpdateEmployerAsync(string employerId, EmployerUpdateRequest updateRequest, HttpContext httpContext);

        /// <summary>
        /// Soft deletes an employer asynchronously.
        /// </summary>
        /// <param name="employerId">The ID of the employer to delete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result indicates the success or failure of the operation.</returns>
        Task<Core.Results.IResult> SoftDeleteEmployerAsync(string employerId);
    }
}
