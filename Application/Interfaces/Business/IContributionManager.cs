using Common.DTOs.Requests;
using Common.DTOs.Responses;
using Common.Models;
using Persistence.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Business
{
    /// <summary>
    /// Interface for managing contributions.
    /// </summary>
    public interface IContributionManager
    {
        /// <summary>
        /// Adds a new contribution asynchronously.
        /// </summary>
        /// <param name="request">The contribution request.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the data result of the contribution DTO.</returns>
        Task<IDataResult<ContributionDTO>> AddContributionAsync(ContributionRequest request);

        /// <summary>
        /// Gets a contribution by its ID asynchronously.
        /// </summary>
        /// <param name="id">The contribution ID.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the data result of the contribution DTO.</returns>
        Task<IDataResult<ContributionDTO>> GetContributionByIdAsync(string id);

        /// <summary>
        /// Gets contributions by member ID asynchronously.
        /// </summary>
        /// <param name="memberId">The member ID.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the data result of the list of contribution DTOs.</returns>
        Task<IDataResult<List<ContributionDTO>>> GetContributionsByMemberAsync(string memberId);

        /// <summary>
        /// Gets the contribution summary by member ID asynchronously.
        /// </summary>
        /// <param name="memberId">The member ID.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the data result of the contribution summary DTO.</returns>
        Task<IDataResult<ContributionSummaryDTO>> GetContributionSummaryAsync(string memberId);

        /// <summary>
        /// Generates statements asynchronously.
        /// </summary>
        /// <param name="request">The statement request.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the data result of the statement result.</returns>
        Task<IDataResult<StatementResult>> GenerateStatementsAsync(StatementRequest request);

        /// <summary>
        /// Processes a withdrawal asynchronously.
        /// </summary>
        /// <param name="request">The withdrawal request.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the data result of the withdrawal result.</returns>
        Task<IDataResult<WithdrawalResult>> ProcessWithdrawalAsync(WithdrawalRequest request);

        /// <summary>
        /// Checks eligibility by member ID asynchronously.
        /// </summary>
        /// <param name="memberId">The member ID.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the data result of the eligibility result DTO.</returns>
        Task<IDataResult<EligibilityResultDTO>> CheckEligibilityAsync(string memberId);

        /// <summary>
        /// Gets unvalidated contributions asynchronously.
        /// </summary>
        /// <param name="cutoffDate">The cutoff date.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the list of unvalidated contributions.</returns>
        Task<List<Contribution>> GetUnvalidatedContributionsAsync(DateTime cutoffDate);

        /// <summary>
        /// Validates a contribution asynchronously.
        /// </summary>
        /// <param name="contributionId">The contribution ID.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the data result of the validation status.</returns>
        Task<IDataResult<string>> ValidateContributionAsync(string contributionId);

        /// <summary>
        /// Gets members approaching eligibility asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the list of members approaching eligibility.</returns>
        Task<List<Member>> GetMembersApproachingEligibilityAsync();

        /// <summary>
        /// Gets accounts for interest calculation asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the list of accounts for interest calculation.</returns>
        Task<List<Account>> GetAccountsForInterestCalculationAsync();

        /// <summary>
        /// Gets the current interest rate asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the current interest rate.</returns>
        Task<decimal> GetCurrentInterestRateAsync();
    }
}
