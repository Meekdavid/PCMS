using Hangfire.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackgroundJobs
{
    public interface IBackgroundJobService
    {
        void ScheduleRecurringJobs();
        Task ValidateContributions(PerformContext context);
        Task UpdateEligibilityStatuses(PerformContext context);
        Task CalculateInterest(PerformContext context);
        Task ProcessFailedTransactions(PerformContext context);
    }
}
