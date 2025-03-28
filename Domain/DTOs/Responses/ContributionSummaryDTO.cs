using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTOs.Responses
{
    public class ContributionSummaryDTO
    {
        public string MemberId { get; set; }
        public decimal TotalContributions { get; set; }
        public decimal MonthlyContributions { get; set; }
        public decimal VoluntaryContributions { get; set; }
        public DateTime? LastContributionDate { get; set; }
        public int ContributionCount { get; set; }
    }
}
