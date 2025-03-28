using Persistence.Concrete;
using Persistence.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.DBModels
{
    public class Contribution :BaseModel
    {
        public Contribution()
        {
            IsValidated = false;
            Status = Status.Active;
        }
        public string ContributionId { get; set; }
        public string MemberId { get; set; }
        public Member Member { get; set; }
        public decimal Amount { get; set; }
        public string PensionAccountNumber { get; set; }
        public ContributionType ContributionType { get; set; } // Monthly or Voluntary
        public bool IsValidated { get; set; }
    }
}
