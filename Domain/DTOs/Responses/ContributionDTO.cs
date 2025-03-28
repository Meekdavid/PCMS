using Persistence.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTOs.Responses
{
    public class ContributionDTO
    {
        public string ContributionId { get; set; }
        public string MemberId { get; set; }
        public string MemberName { get; set; }
        public string MemberProfilePicture { get; set; }
        public string PensionAccountNumber { get; set; }
        public decimal Amount { get; set; }
        public ContributionType ContributionType { get; set; } // Monthly or Voluntary
        public DateTime ContributionDate { get; set; }
        public bool IsValidated { get; set; }
    }
}
