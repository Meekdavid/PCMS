using Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Results;

namespace Core.Results
{
    public class DataResult<T> : Result, IDataResult<T>
    {
        public T Data { get; }
        public DataResult(T data, string ResponseCode, string ResponseDescription) : base(ResponseCode, ResponseDescription)
        {
            Data = data;
        }

        public DataResult(T data, string ResponseCode) : base(ResponseCode)
        {
            Data = data;
        }

        public DataResult(string ResponseCode, string ResponseDescription) : base(ResponseCode, ResponseDescription)
        {
            //Data = data;
        }
    }
}
