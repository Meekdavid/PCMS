using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Common.Literals.StringLiterals;

namespace Core.Results
{
    public class SuccessResult : Result
    {
        public SuccessResult(string ResponseDescription) : base(ResponseCode_Success, ResponseDescription)
        {

        }

        public SuccessResult() : base(ResponseCode_Success)
        {

        }
    }
}
