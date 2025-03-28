using Microsoft.AspNetCore.Identity;
using Persistence.Abstract;
using Persistence.Enums;

namespace Persistence.DBModels
{
    public class Member : IdentityUser<string>, IBaseModel
    {
        public Member()
        {
            Id = Ulid.NewUlid().ToString();
            Accounts = new HashSet<Account>();
            Status = Status.Active;
            EmailConfirmed = true;
            PhoneNumberConfirmed = true;
            RefreshToken = string.Empty;
        }
        //public string MemberId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string NickName { get; set; }
        public string ProfilePicture { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string NationalIdentificationNumber { get; set; }
        public string Address { get; set; }
        public string BankAccountNumber { get; set; }
        public string BankName { get; set; }
        public string RefreshToken { get; set; }
        public DateTime RefreshTokenEndDate { get; set; }
        public string? EmployerId { get; set; }
        public Employer Employer { get; set; }
        public bool IsActive { get; set; }
        public Status Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public DateTime? DeletedDate { get; set; }
        public MembershipType MembershipType { get; set; }        
        public bool IsEligibleForBenefits { get; set; }

        public ICollection<Account> Accounts { get; set; }
    }
}
