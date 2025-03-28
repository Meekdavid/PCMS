using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Services
{
    public interface ICacheService
    {
        Task<T> GetOrSetCacheAsync<T>(string cacheKey, Func<Task<T>> fetchData);
        void Remove(string cacheKey);

    }
}
