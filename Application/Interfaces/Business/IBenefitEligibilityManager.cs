using Common.DTOs.Responses;
using Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Business
{
    /// <summary>
    /// Interface for managing benefit eligibility.
    /// </summary>
    public interface IBenefitEligibilityManager
    {
        /// <summary>
        /// Recalculates the eligibility for a given member.
        /// </summary>
        /// <param name="memberId">The ID of the member.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the eligibility result.</returns>
        Task<IDataResult<EligibilityResultDTO>> RecalculateEligibilityAsync(string memberId);
    }
}
