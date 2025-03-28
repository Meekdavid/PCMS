using Persistence.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTOs.Responses
{
    public class AccountDTO
    {
        public string PensionAccountId { get; set; } // Unique Account Identifier
        public string MemberId { get; set; } // Reference to Member
        public string EmployerId { get; set; } // Reference to Employer (if applicable)
        public string PensionAccountNumber { get; set; } // Pension Account Number
        public AccountType AccountType { get; set; } // Enum for account type (e.g., Pension, Savings, etc.)
        public decimal Balance { get; set; } // Current account balance
        public bool IsRestricted { get; set; }
        public bool IsClosed { get; set; }
    }
}
