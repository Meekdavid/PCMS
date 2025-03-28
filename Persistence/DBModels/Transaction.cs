using Persistence.Concrete;
using Persistence.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Persistence.Enums;

namespace Persistence.DBModels
{
    public class Transaction : BaseModel
    {
        public Transaction()
        {
            Status = Status.Active;
        }
        public string TransactionId { get; set; } // Unique identifier for the transaction
        public string DebitAccountId { get; set; } // Account to debit (Employer account if contribution is from Employer, Individual Account if contribution is from individula)
        public string DebitAccountBankCode { get; set; }
        public string CreditAccountId { get; set; } // NLPC Pension Bannk Account
        public string CreditAccountBankCode { get; set; }
        public int Attempts { get; set; }
        public string MemberId { get; set; } // Reference to the member initiating the transaction
        public string? ContributionId { get; set; } // Reference to the related contribution (if applicable)
        public TransactionType TransactionType { get; set; } // Enum: Contribution, Withdrawal, Transfer, etc.
        public decimal Amount { get; set; } // Amount involved in the transaction
        public TransactionStatus TransactionStatus { get; set; } // Pending, Completed, Failed
        public string? ReferenceNumber { get; set; } // External reference number (if any)
        public string? Description { get; set; } // Additional details about the transaction
        public DateTime TransactionDate { get; set; } // Timestamp of the transaction
        public DateTime? ProcessedDate { get; set; } // When the transaction was processed
        public bool IsReversed { get; set; } // Flag to indicate if transaction was reversed

        // Navigation Properties
        //public virtual Account Account { get; set; }
        //public virtual Member Member { get; set; }
        //public virtual Contribution Contribution { get; set; } // Links to the contribution
    }

}
