using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Results
{
    public class ErrorResult : Result
    {
        public ErrorResult(string ResponseCode, string ResponseDescription) : base(ResponseCode, ResponseDescription)
        {

        }

        public ErrorResult(string ResponseDescription) : base(ResponseDescription)
        {

        }
    }
}
