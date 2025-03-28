using Microsoft.AspNetCore.Http;
using Persistence.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTOs.Requests
{
    public class MemberUpdateRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string NickName { get; set; }
        public IFormFile ProfilePicture { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string NationalIdentificationNumber { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string BankAccountNumber { get; set; }
        public string BankName { get; set; }
        public string Password { get; set; }
        public string? EmployerId { get; set; }
        public MembershipType MembershipType { get; set; }
    }
}
