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
    public class Admin : BaseModel
    {
        public Admin()
        {
            Status = Status.Active;
        }
        [StringLength(50)]
        public string AdminId { get; set; }
        public string MemberId { get; set; }
        public Member Member { get; set; }
    }
}
