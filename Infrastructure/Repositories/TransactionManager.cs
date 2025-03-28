using Application.Interfaces.Business;
using Application.Interfaces.Database;
using Application.Interfaces.General;
using Application.Repositories;
using AutoMapper;
using AutoMapper.Execution;
using Common.ConfigurationSettings;
using Common.DTOs.Responses;
using Common.Models;
using Core.Results;
using Microsoft.Extensions.Logging;
using Persistence.DBModels;
using Persistence.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Common.Literals.StringLiterals;

namespace Infrastructure.Repositories
{
    public class TransactionManager : ITransactionManager
    {
        private readonly ITransactionRepository _transactionDal;
        private readonly IAccountRepository _accountDal;
        private readonly IMemberRepository _memberDal;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<TransactionManager> _logger;

        public TransactionManager(
            ITransactionRepository transactionDal,
            IUnitOfWork unitOfWork,
            ILogger<TransactionManager> logger,
            IMapper mapper,
            IAccountRepository accountDal,
            IMemberRepository memberDal)
        {
            _transactionDal = transactionDal;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
            _accountDal = accountDal;
            _memberDal = memberDal;
        }

        /// <summary>
        /// Processes a contribution transaction asynchronously.
        /// </summary>
        public async Task<IDataResult<TransactionDTO>> ProcessContributionTransactionAsync(
            string memberId,
            string creditAccountId,
            string creditAccountBankCode,
            string debitAccountId,
            string debitAccountBankCode,
            string contributionId,
            decimal amount,
            string remarks,
            AccountType accountType,
            TransactionType transactionType)
        {
            try
            {
                _logger.LogInformation($"About Posting transaction for contribution of {amount} for member {memberId}");
                // In a real system, this would integrate with a payment gateway
                // For this implementation, I'll simulate the transaction

                var transaction = new Transaction
                {
                    CreditAccountBankCode = creditAccountBankCode, // Bank code of the credit account
                    CreditAccountId = creditAccountId, // ID of the credit account
                    DebitAccountBankCode = debitAccountBankCode, // Bank code of the debit account
                    DebitAccountId = debitAccountId, // ID of the debit account
                    MemberId = memberId, // ID of the member making the transaction
                    ContributionId = contributionId, // ID of the contribution
                    TransactionType = transactionType, // Type of the transaction
                    Amount = amount, // Amount of the transaction
                    TransactionStatus = TransactionStatus.Completed, // Status of the transaction
                    ReferenceNumber = Guid.NewGuid().ToString(), // Unique reference number for the transaction
                    Description = remarks, // Description or remarks for the transaction
                    TransactionDate = DateTime.UtcNow, // Date and time of the transaction
                    ProcessedDate = DateTime.UtcNow, // Date and time when the transaction was processed
                    IsReversed = false // Indicates if the transaction is reversed
                };

                await _transactionDal.AddAsync(transaction); // Add the transaction to the database
                await _unitOfWork.SaveChangesAsync(); // Save changes to the database

                _logger.LogInformation($"Transaction processed for contribution {contributionId}");

                return new SuccessDataResult<TransactionDTO>(_mapper.Map<TransactionDTO>(transaction)); // Return the transaction details
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing transaction for contribution {contributionId}");
                return new ErrorDataResult<TransactionDTO>(null, ResponseCode_TransactionProcessingFailed, ResponseMessage_TransactionProcessingFailed); // Return error result
            }
        }

        /// <summary>
        /// Retrieves a list of failed transactions with less than 3 attempts.
        /// </summary>
        public async Task<List<Transaction>> GetFailedTransactionsAsync()
        {
            _logger.LogInformation("Retrieving failed transactions with less than 3 attempts");
            return await _transactionDal.GetAll(
                t => t.TransactionStatus == TransactionStatus.Failed &&
                     t.Attempts < 3 // Max retries
            );
        }

        /// <summary>
        /// Retries a failed transaction asynchronously.
        /// </summary>
        public async Task<IDataResult<TransactionDTO>> RetryFailedTransactionAsync(string transactionId)
        {
            _logger.LogInformation($"Retrying failed transaction with ID: {transactionId}");
            var transaction = await _transactionDal.Get(t => t.TransactionId == transactionId); // Retrieve the transaction by ID
            if (transaction == null)
            {
                _logger.LogWarning($"Transaction with ID: {transactionId} not found");
                return new ErrorDataResult<TransactionDTO>("Transaction not found", "404"); // Return error if transaction not found
            }

            try
            {
                // Simulate retry logic - in reality this would call payment processor
                transaction.TransactionStatus = TransactionStatus.Completed; // Update transaction status to completed
                transaction.ProcessedDate = DateTime.UtcNow; // Update processed date
                transaction.Attempts++; // Increment the number of attempts

                await _transactionDal.Update(transaction); // Update the transaction in the database

                // Update related account balance if needed
                if (transaction.TransactionType == TransactionType.Contribution)
                {
                    var account = await _accountDal.Get(a => a.AccountId == transaction.CreditAccountId); // Retrieve the credit account
                    if (account != null)
                    {
                        account.CurrentBalance += transaction.Amount; // Update the account balance
                        await _accountDal.Update(account); // Update the account in the database
                    }
                }

                _logger.LogInformation($"Transaction with ID: {transactionId} retried successfully");
                return new SuccessDataResult<TransactionDTO>(_mapper.Map<TransactionDTO>(transaction)); // Return the transaction details
            }
            catch (Exception ex)
            {
                transaction.TransactionStatus = TransactionStatus.Failed; // Update transaction status to failed
                transaction.Attempts++; // Increment the number of attempts
                await _transactionDal.Update(transaction); // Update the transaction in the database

                _logger.LogError(ex, $"Failed to retry transaction {transactionId}");
                return new ErrorDataResult<TransactionDTO>("Retry failed", "TX01"); // Return error result
            }
        }

        /// <summary>
        /// Applies interest to an account asynchronously.
        /// </summary>
        public async Task<IDataResult<TransactionDTO>> ApplyInterestAsync(string accountId, string memberId, decimal amount)
        {
            _logger.LogInformation($"Applying interest of {amount} to account {accountId} for member {memberId}");

            try
            {
                // Update account balance
                var account = await _accountDal.Get(a => a.AccountId == accountId); // Retrieve the account by ID
                account.CurrentBalance += amount; // Update the account balance
                await _accountDal.Update(account); // Update the account in the database

                await _unitOfWork.CommitAsync(); // Commit the transaction

                _logger.LogInformation($"Interest of {amount} applied to account {accountId} for member {memberId}");
                return new SuccessDataResult<TransactionDTO>(null); // Return success result
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync(); // Rollback the transaction
                _logger.LogError(ex, $"Error applying interest to account {accountId}");
                return new ErrorDataResult<TransactionDTO>("Interest application failed", "INT01"); // Return error result
            }
        }
    }
}
