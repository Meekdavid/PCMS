using Common.DTOs.Responses;
using Persistence.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.General
{
    public interface IFileManager
    {
        Task<StatementResult> GeneratePdfStatement(Member member, List<Contribution> contributions, DateTime startDate, DateTime endDate);
        Task<StatementResult> GenerateExcelStatement(Member member, List<Contribution> contributions, DateTime startDate, DateTime endDate);
    }
}
