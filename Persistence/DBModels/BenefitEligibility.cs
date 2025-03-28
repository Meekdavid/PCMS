using Persistence.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Persistence.Enums;

namespace Persistence.DBModels
{
    public class BenefitEligibility : BaseModel
    {
        public BenefitEligibility()
        {
            Status = Status.Active;
        }
        public string BenefitEligibilityId { get; set; }
        public BenefitType BenefitType { get; set; }
        public string MemberId { get; set; }
        public Member Member { get; set; }
        public bool IsEligible { get; set; }
        public DateTime EligibilityDate { get; set; }
        public string EligibilityReason { get; set; }
    }

}
