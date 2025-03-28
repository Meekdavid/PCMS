using Core.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public interface IDataResult<T> : IResult
    {
        T Data { get; }
    }
}
