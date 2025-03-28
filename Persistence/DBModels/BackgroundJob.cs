using Persistence.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.DBModels
{
    public class BackgroundJob
    {
        public BackgroundJob()
        {
            Id = Ulid.NewUlid().ToString();
            Status = JobStatus.Pending;
            CreatedAt = DateTime.UtcNow;
        }
        public string Id { get; set; }
        public BackgroundJobType JobType { get; set; } // Validation, Notifications, Interest Calculation
        public JobStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
