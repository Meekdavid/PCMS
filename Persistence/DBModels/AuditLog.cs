using Persistence.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.DBModels
{
    public class AuditLog :BaseModel
    {
        public string AuditLogId { get; set; }
        public string Action { get; set; } // "Created Member", "Processed Contribution"
        public string? MemberId { get; set; }
        public string? Details { get; set; }
    }


}
