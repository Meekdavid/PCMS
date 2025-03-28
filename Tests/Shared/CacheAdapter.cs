using Common.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Shared
{
    public interface IICacheServiceAdapter
    {
        Task<T> GetOrSetCacheAsync<T>(string key, Func<Task<T>> factory);
    }

    public class ICacheServiceAdapter : IICacheServiceAdapter
    {
        private readonly ICacheService _cacheService;

        public ICacheServiceAdapter(ICacheService cacheService)
        {
            _cacheService = cacheService;
        }

        public Task<T> GetOrSetCacheAsync<T>(string key, Func<Task<T>> factory)
        {
            return _cacheService.GetOrSetCacheAsync(key, factory);
        }
    }
}
