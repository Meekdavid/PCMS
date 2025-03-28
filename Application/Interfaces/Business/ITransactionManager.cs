using Common.Models;
using Persistence.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.DTOs.Responses;
using Persistence.DBModels;

namespace Application.Interfaces.Business
{
    /// <summary>
    /// Interface for managing transactions.
    /// </summary>
    public interface ITransactionManager
    {
        /// <summary>
        /// Processes a contribution transaction asynchronously.
        /// </summary>
        /// <param name="memberId">The ID of the member.</param>
        /// <param name="creditAccountId">The ID of the credit account.</param>
        /// <param name="creditAccountBankCode">The bank code of the credit account.</param>
        /// <param name="debitAccountId">The ID of the debit account.</param>
        /// <param name="debitAccountBankCode">The bank code of the debit account.</param>
        /// <param name="contributionId">The ID of the contribution.</param>
        /// <param name="amount">The amount of the transaction.</param>
        /// <param name="remarks">Remarks for the transaction.</param>
        /// <param name="accountType">The type of the account.</param>
        /// <param name="transactionType">The type of the transaction.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the data result of the transaction DTO.</returns>
        Task<IDataResult<TransactionDTO>> ProcessContributionTransactionAsync(
            string memberId,
            string creditAccountId,
            string creditAccountBankCode,
            string debitAccountId,
            string debitAccountBankCode,
            string contributionId,
            decimal amount,
            string remarks,
            AccountType accountType,
            TransactionType transactionType);

        /// <summary>
        /// Gets the list of failed transactions asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the list of failed transactions.</returns>
        Task<List<Transaction>> GetFailedTransactionsAsync();

        /// <summary>
        /// Retries a failed transaction asynchronously.
        /// </summary>
        /// <param name="transactionId">The ID of the transaction to retry.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the data result of the transaction DTO.</returns>
        Task<IDataResult<TransactionDTO>> RetryFailedTransactionAsync(string transactionId);

        /// <summary>
        /// Applies interest to an account asynchronously.
        /// </summary>
        /// <param name="accountId">The ID of the account.</param>
        /// <param name="memberId">The ID of the member.</param>
        /// <param name="amount">The amount of interest to apply.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the data result of the transaction DTO.</returns>
        Task<IDataResult<TransactionDTO>> ApplyInterestAsync(string accountId, string memberId, decimal amount);
    }
}
