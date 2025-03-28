using Persistence.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTOs.Responses
{
    public class EligibilityResultDTO
    {
        public EligibilityResultDTO()
        {
            EvaluationDate = DateTime.UtcNow;
        }
        public string MemberId { get; set; }
        public BenefitType BenefitType { get; set; }
        public bool IsEligible { get; set; }
        public DateTime EvaluationDate { get; set; }
        public DateTime EligibilityDate { get; set; }
        public List<string> PassedRequirements { get; set; } = new();
        public List<string> FailedRequirements { get; set; } = new();
        public string Summary => IsEligible
            ? $"Eligible for {BenefitType} benefits"
            : $"Not eligible for {BenefitType} benefits";
    }
}
