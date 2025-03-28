using AutoMapper.Execution;
using Persistence.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTOs.Requests
{
    public class ContributionRequest
    {
        public string MemberId { get; set; }
        public AccountType AccountType { get; set; } // Decides which bank account to deduct money, personal or employer account?
        public decimal Amount { get; set; }

        public ContributionType ContributionType { get; set; } // Monthly or Voluntary
    }
}
