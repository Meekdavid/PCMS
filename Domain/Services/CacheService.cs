using Common.ConfigurationSettings;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Services;

public class CacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<ICacheService> _logger;

    // Default cache duration (can be adjusted from the config)
    private readonly TimeSpan _defaultCacheDuration = TimeSpan.FromMinutes(ConfigSettings.ApplicationSetting.CacheDuration);

    public CacheService(IMemoryCache cache, ILogger<ICacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves cached data if available; otherwise, fetches, caches, and returns new data.
    /// </summary>
    /// <typeparam name="T">Type of the data to cache.</typeparam>
    /// <param name="cacheKey">Unique key for the cache entry.</param>
    /// <param name="fetchData">Function to fetch data when not found in cache.</param>
    /// <returns>Returns the cached or newly fetched data.</returns>
    public async Task<T> GetOrSetCacheAsync<T>(string cacheKey, Func<Task<T>> fetchData)
    {
        // Try to get data from cache
        if (_cache.TryGetValue(cacheKey, out T cachedData))
        {
            _logger.LogInformation($"Cache hit: Returning cached data for key '{cacheKey}'.");
            return cachedData;
        }

        _logger.LogInformation($"Cache miss: Fetching data for key '{cacheKey}'...");

        // Fetch new data since it's not cached
        cachedData = await fetchData();

        // Store the fetched data in cache for the default duration
        _cache.Set(cacheKey, cachedData, _defaultCacheDuration);

        _logger.LogInformation($"Data cached successfully for key '{cacheKey}' with expiration {_defaultCacheDuration.TotalMinutes} minutes.");

        return cachedData;
    }

    /// <summary>
    /// Removes the cached data associated with the specified key.
    /// </summary>
    /// <param name="cacheKey">The unique key for the cache entry to remove.</param>
    public void Remove(string cacheKey)
    {
        if (string.IsNullOrWhiteSpace(cacheKey))
        {
            _logger.LogWarning("Attempted to remove cache entry with an empty or null key.");
            return;
        }

        _cache.Remove(cacheKey);
        _logger.LogInformation($"Cache entry for key '{cacheKey}' has been removed.");
    }

}
