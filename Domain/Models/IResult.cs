using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Results
{
    public interface IResult
    {
        public string ResponseCode { get; } // Only for read
        public string ResponseDescription { get; }
    }
}
