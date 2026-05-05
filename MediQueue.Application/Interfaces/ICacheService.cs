// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Interfaces\ICacheService.cs
using System;
using System.Threading.Tasks;

namespace MediQueue.Application.Interfaces;

/// <summary>
/// Service for interacting with a distributed or memory cache.
/// </summary>
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan expiry);
    Task RemoveAsync(string key);
    Task RemoveByPrefixAsync(string prefix);
}
