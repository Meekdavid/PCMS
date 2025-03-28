using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Results
{
    public class Result : IResult
    {
        public string ResponseCode { get; }

        public string ResponseDescription { get; }
        public Result(string ResponseCode, string responseDescription) : this(ResponseCode)
        {
            ResponseDescription = responseDescription;
        }

        public Result(string responseCode)
        {
            ResponseCode = responseCode;
        }
    }
}
