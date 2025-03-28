using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Common.Literals.StringLiterals;

namespace Core.Results
{
    public class SuccessDataResult<T> : DataResult<T>
    {
        public SuccessDataResult(T data) : base(data, ResponseCode_Success, ResponseMessage_Success)
        {

        }

        public SuccessDataResult(T data, string ResponseDescription) : base(ResponseCode_Success, ResponseDescription)
        {

        }

    }
}
