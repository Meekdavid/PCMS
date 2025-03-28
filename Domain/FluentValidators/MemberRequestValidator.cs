using Common.ConfigurationSettings;
using Common.DTOs.Requests;
using Common.Services.ExtensionServices;
using FluentValidation;
using Persistence.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.FluentValidators
{
    public class MemberRequestValidator : AbstractValidator<MemberRequest>
    {
        public MemberRequestValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(50).WithMessage("First name cannot exceed 50 characters.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters.");

            RuleFor(x => x.DateOfBirth)
                .NotEmpty().WithMessage("Date of birth is required.")
                .Must(dob => dob.CalculateAge() >= ConfigSettings.ApplicationSetting.MinimunRequiredAge && dob.CalculateAge() <= ConfigSettings.ApplicationSetting.MaximumRequiredAge)
                .WithMessage("Age must be between 18 and 70.");

            RuleFor(x => x.NationalIdentificationNumber)
                .NotEmpty().WithMessage("National Identification Number is required.")
                .Matches(@"^\d{11,20}$").WithMessage("Invalid National Identification Number format.");

            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Address is required.");

            RuleFor(x => x.ProfilePicture)
                .Must(ValidationLogics.BeAValidImage).When(x => x.ProfilePicture != null)
                .WithMessage("Invalid image file. Only JPG, JPEG, and PNG formats under 1MB are allowed.");

            RuleFor(x => x.EmployerId)
                .NotEmpty().WithMessage("EmployerId is required when MembershipType is Employee.")
                .When(x => x.MembershipType == MembershipType.Employee);
        }
    }
}
