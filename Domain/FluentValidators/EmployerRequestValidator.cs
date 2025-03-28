using Common.DTOs.Requests;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.FluentValidators
{
    public class EmployerRequestValidator : AbstractValidator<EmployerRequest>
    {
        public EmployerRequestValidator()
        {
            RuleFor(x => x.CompanyName)
                .NotEmpty().WithMessage("Company name is required.");

            RuleFor(x => x.RegistrationNumber)
                .NotEmpty().WithMessage("Registration number is required.");

            RuleFor(x => x.TaxIdentificationNumber)
                .NotEmpty().WithMessage("Tax Identification Number is required.");

            RuleFor(x => x.ContactEmail)
                .NotEmpty().WithMessage("Contact email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.ContactPhone)
                .NotEmpty().WithMessage("Contact phone is required.")
                .Matches(@"^\+?[0-9]{10,15}$").WithMessage("Invalid phone number format.");

            RuleFor(x => x.Industry)
                .NotEmpty().WithMessage("Industry is required.");

            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Address is required.");

            RuleFor(x => x.CompanyProfileImage)
                .Must(ValidationLogics.BeAValidImage).When(x => x.CompanyProfileImage != null)
                .WithMessage("Invalid image file. Only JPG, JPEG, and PNG formats under 1MB are allowed.");

            RuleFor(x => x.IsActive)
                .NotNull().WithMessage("Company status is required.");
        }
    }
}
