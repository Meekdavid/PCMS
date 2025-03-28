using Hangfire.Server;
using Hangfire;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Application.Interfaces.Business;
using static Common.Literals.StringLiterals;
using Common.ConfigurationSettings;

namespace BackgroundJobs
{
    public class BackgroundJobService : IBackgroundJobService
    {
        private readonly IBenefitEligibilityManager _eligibilityManager;
        private readonly IContributionManager _contributionManager;
        private readonly ITransactionManager _transactionManager;
        private readonly INotificationManager _notificationService;
        private readonly ILogger<BackgroundJobService> _logger;
        private readonly IRecurringJobManager _recurringJobManager;

        // Constructor to initialize dependencies
        public BackgroundJobService(
            IContributionManager contributionManager,
            ITransactionManager transactionManager,
            INotificationManager notificationService,
            ILogger<BackgroundJobService> logger,
            IBenefitEligibilityManager eligibilityManager,
            IRecurringJobManager recurringJobManager)
        {
            _contributionManager = contributionManager;
            _transactionManager = transactionManager;
            _notificationService = notificationService;
            _logger = logger;
            _eligibilityManager = eligibilityManager;
            _recurringJobManager = recurringJobManager;
        }

        // Method to schedule recurring jobs using Hangfire
        public void ScheduleRecurringJobs()
        {
            // Contribution validation - runs daily at 2 AM
            _recurringJobManager.AddOrUpdate(
                "validate-contributions",
                () => ValidateContributions(null),
                Cron.Daily(2));

            // Eligibility updates - runs monthly on the 1st at 3 AM
            _recurringJobManager.AddOrUpdate(
                "update-eligibility",
                () => UpdateEligibilityStatuses(null),
                Cron.Monthly(1, 3));

            // Interest calculation - runs monthly on the last day at 4 AM
            _recurringJobManager.AddOrUpdate(
                "calculate-interest",
                () => CalculateInterest(null),
                Cron.Monthly(28, 4));

            // Failed transaction processing - runs every 30 minutes
            _recurringJobManager.AddOrUpdate(
                "process-failed-transactions",
                () => ProcessFailedTransactions(null),
                "*/30 * * * *");
        }

        // Method to validate contributions
        [AutomaticRetry(Attempts = 3, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public async Task ValidateContributions(PerformContext context)
        {
            _logger.LogInformation("Starting contribution validation job");

            try
            {
                // Get unvalidated contributions from the last 24 hours
                var cutoffDate = DateTime.UtcNow.AddHours(-24);
                var unvalidated = await _contributionManager.GetUnvalidatedContributionsAsync(cutoffDate);

                foreach (var contribution in unvalidated)
                {
                    try
                    {
                        var validationResult = await _contributionManager.ValidateContributionAsync(contribution.ContributionId);

                        if (validationResult.ResponseCode != ResponseCode_Success)
                        {
                            await _notificationService.SendValidationAlertAsync(
                                contribution.MemberId,
                                $"Contribution {contribution.ContributionId} failed validation: {validationResult.ResponseDescription}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error validating contribution {contribution.ContributionId}");
                    }
                }

                _logger.LogInformation($"Processed {unvalidated.Count} contributions");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in contribution validation job");
                throw;
            }
        }

        // Method to update eligibility statuses
        [AutomaticRetry(Attempts = 3, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public async Task UpdateEligibilityStatuses(PerformContext context)
        {
            _logger.LogInformation("Starting eligibility update job");

            try
            {
                var members = await _contributionManager.GetMembersApproachingEligibilityAsync();

                foreach (var member in members)
                {
                    try
                    {
                        var result = await _eligibilityManager.RecalculateEligibilityAsync(member.Id);

                        if (result.Data.IsEligible)
                        {
                            await _notificationService.SendEligibilityNotificationAsync(
                                member.Id,
                                "You are now eligible for pension benefits");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error updating eligibility for member {member.Id}");
                    }
                }

                _logger.LogInformation($"Updated eligibility for {members.Count} members");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in eligibility update job");
                throw;
            }
        }

        // Method to calculate interest
        [AutomaticRetry(Attempts = 3, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public async Task CalculateInterest(PerformContext context)
        {
            _logger.LogInformation("Starting interest calculation job");

            try
            {
                var accounts = await _contributionManager.GetAccountsForInterestCalculationAsync();
                var annualRate = ConfigSettings.ApplicationSetting.PensionAnualRate;

                // Convert annual rate to monthly
                decimal monthlyRate = annualRate / 12 / 100; // Divide by 100 to convert % to decimal

                foreach (var account in accounts)
                {
                    try
                    {
                        // Calculate MONTHLY interest
                        decimal interestAmount = account.CurrentBalance * monthlyRate;

                        var result = await _transactionManager.ApplyInterestAsync(
                            account.AccountId,
                            account.MemberId,
                            interestAmount);

                        if (result.ResponseCode == ResponseCode_Success)
                        {
                            await _notificationService.SendInterestNotificationAsync(
                                account.MemberId,
                                $"Monthly interest of {interestAmount:C} applied. Rate: {monthlyRate * 100:F2}%");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error processing account {account.AccountId}");
                    }
                }

                _logger.LogInformation($"Processed {accounts.Count} accounts");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Interest job failed");
                throw;
            }
        }


        // Method to process failed transactions
        [AutomaticRetry(Attempts = 3, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public async Task ProcessFailedTransactions(PerformContext context)
        {
            _logger.LogInformation("Starting failed transaction processing job");

            try
            {
                var failedTransactions = await _transactionManager.GetFailedTransactionsAsync();

                foreach (var transaction in failedTransactions)
                {
                    try
                    {
                        var retryResult = await _transactionManager.RetryFailedTransactionAsync(transaction.TransactionId);

                        if (retryResult.ResponseCode != ResponseCode_Success)
                        {
                            await _notificationService.SendTransactionAlertAsync(
                                transaction.MemberId,
                                $"Transaction {transaction.TransactionId} failed: {retryResult.ResponseDescription}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error retrying transaction {transaction.TransactionId}");
                    }
                }

                _logger.LogInformation($"Processed {failedTransactions.Count} failed transactions");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in failed transaction job");
                throw;
            }
        }
    }
}
