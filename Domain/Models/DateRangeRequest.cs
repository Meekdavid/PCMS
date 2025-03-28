using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    /// <summary>
    /// Represents a date range for filtering records.
    /// </summary>
    public class DateRangeRequest
    {
        /// <summary>
        /// The start date of the range (inclusive). Format: YYYY-MM-DD.
        /// </summary>
        [FromHeader]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// The end date of the range (inclusive). Format: YYYY-MM-DD.
        /// </summary>
        [FromHeader]
        public DateTime EndDate { get; set; }
    }
}
