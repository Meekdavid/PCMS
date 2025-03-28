using Persistence.DBModels;
using Persistence.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTOs.Responses
{
    public class MemberDTO
    {
        public string MemberId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ProfilePicture { get; set; }
        public string Role { get; set; } // Admin, Member, Employer
        public DateTime DateOfBirth { get; set; }
        public string NationalIdentificationNumber { get; set; }
        public string Address { get; set; }
        public string BankAccountNumber { get; set; }
        public string BankName { get; set; }
        public string? EmployerId { get; set; }
        public Employer Employer { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public DateTime? DeletedDate { get; set; }
        public MembershipType MembershipType { get; set; }
        public decimal TotalContributions { get; set; }
        public bool IsEligibleForBenefits { get; set; }

        public ICollection<Account> Accounts { get; set; }
    }
}
