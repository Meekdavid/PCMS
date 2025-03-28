using Persistence.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Abstract
{
    public interface IBaseModel
    {
        Status Status
        {
            get { return Status; }
            private set { Status = value; }
        }
        DateTime CreatedDate { get; set; }
        DateTime? ModifiedDate { get; set; }
        DateTime? DeletedDate { get; set; }
    }
}
