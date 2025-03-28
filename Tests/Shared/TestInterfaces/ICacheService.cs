using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Shared.TestInterfaces
{
    public interface IICacheService
    {
        Task<T> GetOrSetCacheAsync<T>(string cacheKey, Func<Task<T>> func);
    }
}
