using Persistence.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTOs.Responses
{
    public class TransactionDTO
    {
        public string TransactionId { get; set; }
        public string AccountId { get; set; }
        public string MemberId { get; set; }
        public string ContributionId { get; set; }
        public TransactionType TransactionType { get; set; }
        public decimal Amount { get; set; }
        public TransactionStatus TransactionStatus { get; set; }
        public string ReferenceNumber { get; set; }
        public string Description { get; set; }
        public DateTime TransactionDate { get; set; }
    }
}
