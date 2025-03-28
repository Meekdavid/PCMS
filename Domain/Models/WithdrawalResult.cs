using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class WithdrawalResult
    {
        public string TransactionId { get; set; }
        public string MemberId { get; set; }
        public decimal Amount { get; set; }
        public decimal NewBalance { get; set; }
        public DateTime ProcessedDate { get; set; }
    }
}
