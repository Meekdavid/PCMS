using Common.DTOs.Requests;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.FluentValidators
{
    public class ContributionRequestValidator : AbstractValidator<ContributionRequest>
    {
        public ContributionRequestValidator()
        {
            RuleFor(x => x.MemberId)
                .NotEmpty().WithMessage("MemberId is required.");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than zero.");

            RuleFor(x => x.ContributionType)
                .IsInEnum().WithMessage("Invalid Contribution Type.");
        }
    }
}
