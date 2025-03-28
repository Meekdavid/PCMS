using Persistence.Concrete;
using Persistence.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.DBModels
{
    public class Account : BaseModel
    {
        public Account()
        {
            Status = Status.Active;
            IsRestricted = false;
            IsClosed = false;
            CurrentBalance = 0;
            TotalContributions = 0;
            EmployerId = "N/A";
        }
        public string AccountId { get; set; } // Unique Account Identifier
        public string MemberId { get; set; } // Reference to Member
        public string EmployerId { get; set; } // Reference to Employer (if applicable)
        public string PensionAccountNumber { get; set; } // Pension Account Number
        public AccountType AccountType { get; set; } // Enum for account type (e.g., Pension, Savings, etc.)
        public decimal TotalContributions { get; set; }
        public decimal CurrentBalance { get; set; }
        public bool IsRestricted { get; set; }
        public bool IsClosed { get; set; }

        // Navigation Properties
        public virtual Member Member { get; set; }
        public virtual Employer Employer { get; set; }
    }
}
