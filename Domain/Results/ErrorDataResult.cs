using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Results
{
    public class ErrorDataResult<T> : DataResult<T>
    {
        public ErrorDataResult(T data, string ResponseCode, string ResponseDescription) : base(data, ResponseCode, ResponseDescription)
        {

        }

        //public ErrorDataResult(T data, string ResponseCode) : base(data, ResponseCode)
        //{

        //}

        public ErrorDataResult(string ResponseCode, string ResponseDescription) : base(default, ResponseCode, ResponseDescription)
        {

        }
    }
}
