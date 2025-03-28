using Persistence.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Persistence.Enums;

namespace Persistence.DBModels
{
    public class Employer : BaseModel
    {
        public Employer()
        {
            Employees = new HashSet<Member>();
            IsActive = true;
            Status = Status.Active;
        }

        public string EmployerId { get; set; }
        public string CompanyName { get; set; }
        public string CompanyProfileImage { get; set; }
        public string RegistrationNumber { get; set; } // e.g., CAC Number
        public string TaxIdentificationNumber { get; set; } // TIN for tax purposes
        public string Industry { get; set; } // Industry type (e.g., Finance, Technology)
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }
        public string WebsiteUrl { get; set; }
        public string Address { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string BankAccountNumber { get; set; }
        public string BankName { get; set; }
        public int NumberOfEmployees { get; set; } // Approximate number of employees
        public decimal PensionContributionRate { get; set; } // Employer pension contribution rate (e.g., 7.5%)
        public bool IsActive { get; set; }

        // Navigation Property
        public ICollection<Member> Employees { get; set; }
    }

}
