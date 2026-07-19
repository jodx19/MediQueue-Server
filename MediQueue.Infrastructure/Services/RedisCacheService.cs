using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using MediQueue.Application.Interfaces;

namespace MediQueue.Infrastructure.Services;

public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    public RedisCacheService(IDistributedCache cache, IConnectionMultiplexer connectionMultiplexer)
    {
        _cache = cache;
        _connectionMultiplexer = connectionMultiplexer;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan expiry)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiry
        };
        var json = JsonSerializer.Serialize(value);
        await _cache.SetStringAsync(key, json, options);
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var json = await _cache.GetStringAsync(key);
        if (string.IsNullOrEmpty(json))
        {
            return default;
        }
        return JsonSerializer.Deserialize<T>(json);
    }

    public async Task RemoveAsync(string key)
    {
        await _cache.RemoveAsync(key);
    }

    public async Task RemoveByPrefixAsync(string prefix)
    {
        var endpoints = _connectionMultiplexer.GetEndPoints();
        foreach (var endpoint in endpoints)
        {
            var server = _connectionMultiplexer.GetServer(endpoint);
            // In a production environment, SCAN is preferred over KEYS for performance.
            // StackExchange.Redis's Keys method automatically chooses SCAN if the server supports it.
            var keys = server.Keys(pattern: prefix + "*");
            foreach (var key in keys)
            {
                await _cache.RemoveAsync(key!);
            }
        }
    }
}
