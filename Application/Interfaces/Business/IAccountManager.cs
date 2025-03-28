using Common.DTOs.Requests;
using Common.DTOs.Responses;
using Common.Models;
using Common.Pagination;
using Persistence.DBModels;
using Persistence.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Business
{
    /// <summary>
    /// Interface for managing accounts.
    /// </summary>
    public interface IAccountManager
    {
        /// <summary>
        /// Retrieves posting accounts based on account type and member.
        /// </summary>
        /// <param name="accountType">Type of the account.</param>
        /// <param name="member">Member information.</param>
        /// <returns>Posting accounts.</returns>
        Task<PostingAccounts> RetrievePostingAccounts(AccountType accountType, Member member);

        /// <summary>
        /// Retrieves all accounts with pagination.
        /// </summary>
        /// <param name="pageIndex">Index of the page.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <returns>Paginated list of account DTOs.</returns>
        Task<IDataResult<PaginatedList<AccountDTO>>> RetrieveAllAccountsAsync(int pageIndex, int pageSize);

        /// <summary>
        /// Retrieves an account by its ID.
        /// </summary>
        /// <param name="accountId">ID of the account.</param>
        /// <returns>Account DTO.</returns>
        Task<IDataResult<AccountDTO>> RetrieveAccountByIdAsync(string accountId);

        /// <summary>
        /// Adds a new account.
        /// </summary>
        /// <param name="request">New account request.</param>
        /// <returns>Account DTO.</returns>
        Task<IDataResult<AccountDTO>> AddNewAccountAsync(NewAccountRequest request);

        /// <summary>
        /// Retrieves accounts for a specific member with pagination.
        /// </summary>
        /// <param name="memberId">ID of the member.</param>
        /// <returns>Paginated list of account DTOs.</returns>
        Task<IDataResult<PaginatedList<AccountDTO>>> GetAccountsForSpecificMemberAsync(string memberId);
    }
}
