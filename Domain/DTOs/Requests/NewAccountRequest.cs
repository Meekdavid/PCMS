using Persistence.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTOs.Requests
{
    public class NewAccountRequest
    {
        public string MemberId { get; set; } // Reference to Member
        public AccountType AccountType { get; set; } // Enum for account type (e.g., Pension, Savings, etc.)
    }
}
