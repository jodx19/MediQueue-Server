using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using MediQueue.Application.Interfaces;
using StackExchange.Redis;

namespace MediQueue.Infrastructure.ExternalServices;

public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    public RedisCacheService(IDistributedCache cache, IConnectionMultiplexer connectionMultiplexer)
    {
        _cache = cache;
        _connectionMultiplexer = connectionMultiplexer;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var cachedData = await _cache.GetStringAsync(key);
        if (string.IsNullOrEmpty(cachedData))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(cachedData);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan expiry)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiry
        };

        var serializedData = JsonSerializer.Serialize(value);
        await _cache.SetStringAsync(key, serializedData, options);
    }

    public async Task RemoveAsync(string key)
    {
        await _cache.RemoveAsync(key);
    }

    public async Task RemoveByPrefixAsync(string prefix)
    {
        // Using StackExchange.Redis directly to scan and delete by prefix
        var endpoints = _connectionMultiplexer.GetEndPoints();
        foreach (var endpoint in endpoints)
        {
            var server = _connectionMultiplexer.GetServer(endpoint);
            var keys = server.Keys(pattern: $"{prefix}*");
            
            var db = _connectionMultiplexer.GetDatabase();
            foreach (var key in keys)
            {
                await db.KeyDeleteAsync(key);
            }
        }
    }
}
