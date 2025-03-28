using Persistence.Concrete;
using Persistence.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.DBModels
{
    public class EligibilityRule : BaseModel
    {
        public EligibilityRule()
        {
            Status = Status.Active;
            RuleName = string.Empty;
            Description = string.Empty;
            BenefitType = BenefitType.Retirement; // Default to Pension type
            ThresholdValue = 0; // Explicit null for nullable decimal
            IsBooleanRule = false;
            EvaluationOrder = 0;
            IsActive = true;
            ErrorCode = string.Empty;
            ValidationFunction = string.Empty;
        }

        /// <summary>
        /// Unique identifier for the rule
        /// </summary>
        public string EligibilityRuleId { get; set; }

        /// <summary>
        /// Machine-readable name for rule processing
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string RuleName { get; set; }

        /// <summary>
        /// Human-readable description
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string Description { get; set; }

        /// <summary>
        /// The type of benefit this rule applies to
        /// </summary>
        public BenefitType BenefitType { get; set; }

        /// <summary>
        /// The minimum threshold value for this rule
        /// </summary>
        public decimal? ThresholdValue { get; set; }

        /// <summary>
        /// Whether this rule evaluates a boolean condition
        /// </summary>
        public bool IsBooleanRule { get; set; }

        /// <summary>
        /// The order in which rules should be evaluated
        /// </summary>
        public int EvaluationOrder { get; set; }

        /// <summary>
        /// Whether this rule is currently active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// The error code to return if this rule fails
        /// </summary>
        [MaxLength(20)]
        public string ErrorCode { get; set; }

        /// <summary>
        /// Custom validation function name (if applicable)
        /// </summary>
        [MaxLength(100)]
        public string ValidationFunction { get; set; }

        // Navigation property
        // public virtual ICollection<BenefitEligibility> BenefitEligibilities { get; set; }
    }
}
