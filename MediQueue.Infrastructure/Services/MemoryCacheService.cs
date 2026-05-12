using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using MediQueue.Application.Interfaces;

namespace MediQueue.Infrastructure.Services;

/// <summary>
/// In-process memory cache fallback for ICacheService.
/// Used automatically by Infrastructure DI when Redis is unavailable.
/// </summary>
public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;

    public MemoryCacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task SetAsync<T>(string key, T value, TimeSpan expiry)
    {
        _cache.Set(key, value, expiry);
        return Task.CompletedTask;
    }

    public Task<T?> GetAsync<T>(string key)
    {
        _cache.TryGetValue(key, out T? value);
        return Task.FromResult(value);
    }

    public Task RemoveAsync(string key)
    {
        _cache.Remove(key);
        return Task.CompletedTask;
    }

    public Task RemoveByPrefixAsync(string prefix)
    {
        // IMemoryCache does not support pattern-based eviction.
        // In production, migrate to Redis for this feature.
        return Task.CompletedTask;
    }
}
