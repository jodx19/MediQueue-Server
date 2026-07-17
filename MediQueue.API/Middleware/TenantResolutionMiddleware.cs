using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MediQueue.Application.Interfaces;
using MediQueue.Domain.Entities;
using MediQueue.Domain.Interfaces;

namespace MediQueue.API.Middleware;

/// <summary>
/// Resolves tenant from subdomain on every request.
/// Must run BEFORE UseAuthentication() in the pipeline.
///
/// Flow:
///   Host: clinic1.mediqueue.com → subdomain = "clinic1"
///   Host: localhost:5000        → dev mode (resolves first active tenant)
///   Host: 127.0.0.1:5000        → dev mode
///
/// Security note:
///   The cached <see cref="TenantCacheEntry"/> is re-validated against
///   <see cref="TenantCacheEntry.CanAccess"/> on EVERY cache hit. A tenant that
///   has been suspended or whose trial/subscription has expired will be blocked
///   even while the cache entry is still warm — no need to wait for TTL.
/// </summary>
public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantResolutionMiddleware> _logger;

    // Paths that don't need tenant resolution
    private static readonly string[] _excludedPaths =
    [
        "/health",
        "/swagger",
        "/hubs"
    ];

    // Cache TTL for tenant resolution entries.
    public static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan DevCacheTtl = TimeSpan.FromMinutes(10);

    public TenantResolutionMiddleware(
        RequestDelegate next,
        ILogger<TenantResolutionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ITenantContext tenantContext,
        ITenantRepository tenantRepository,
        ICacheService cacheService)
    {
        // Skip excluded paths
        var path = context.Request.Path.Value ?? string.Empty;
        if (_excludedPaths.Any(p =>
            path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        var host = context.Request.Host.Host.ToLowerInvariant();
        var tenantCtx = (MediQueue.API.Services.TenantContext)tenantContext;

        // Dev mode: localhost or IP
        if (host is "localhost" or "127.0.0.1" or "::1" ||
            host.StartsWith("192.168.") ||
            host.StartsWith("10."))
        {
            var devOk = await TryResolveDevTenantAsync(
                tenantRepository, cacheService, tenantCtx, context);

            if (devOk)
            {
                tenantCtx.Subdomain = "dev";
                await _next(context);
            }
            return;
        }

        // Extract subdomain: clinic1.mediqueue.com → "clinic1"
        var subdomain = ExtractSubdomain(host);
        if (string.IsNullOrEmpty(subdomain))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new
            {
                isSuccess = false,
                data = (object?)null,
                message = "Invalid tenant domain.",
                errors = new[] { "Could not resolve tenant from host." }
            });
            return;
        }

        // Cache key: tenant:{subdomain}
        var cacheKey = $"tenant:{subdomain}";

        try
        {
            // Try cache first. The cached entry is a snapshot of the tenant's
            // access-relevant fields; we re-validate access on every hit.
            var cached = await cacheService
                .GetAsync<TenantCacheEntry?>(cacheKey);

            if (cached is not null)
            {
                // Stale-cache guard: a previously-active tenant may have been
                // suspended or its subscription expired since this entry was
                // cached. Block the request before trusting the cached TenantId.
                if (!cached.CanAccess())
                {
                    await WriteTenantSuspendedAsync(context);
                    return;
                }

                tenantCtx.TenantId = cached.TenantId;
                tenantCtx.Subdomain = subdomain;
                await _next(context);
                return;
            }

            // Cache miss → lookup from DB with full checks
            var tenant = await tenantRepository
                .GetBySubdomainAsync(subdomain);

            if (tenant == null || !tenant.IsActive)
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsJsonAsync(new
                {
                    isSuccess = false,
                    data = (object?)null,
                    message = "Clinic not found or inactive.",
                    errors = new[] { $"No clinic found for: {subdomain}" }
                });
                return;
            }

            if (!tenant.CanAccess())
            {
                context.Response.StatusCode = 402;
                await context.Response.WriteAsJsonAsync(new
                {
                    isSuccess = false,
                    data = (object?)null,
                    message = "Subscription required.",
                    errors = new[] { "Trial expired. Please subscribe." }
                });
                return;
            }

            // Cache the FULL snapshot (not just TenantId) so subsequent cache
            // hits can re-validate IsActive / subscription without a DB hit.
            var entry = new TenantCacheEntry
            {
                TenantId           = tenant.Id,
                TenantSlug         = tenant.Subdomain,
                IsActive           = tenant.IsActive,
                SubscriptionEndsAt = tenant.SubscriptionEndsAt,
                TrialEndsAt        = tenant.TrialEndsAt
            };
            await cacheService.SetAsync(cacheKey, entry, CacheTtl);

            tenantCtx.TenantId = tenant.Id;
            tenantCtx.Subdomain = subdomain;
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tenant resolution failed for host {Host}", host);
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new
            {
                isSuccess = false,
                data = (object?)null,
                message = "Tenant resolution failed.",
                errors = new[] { "Internal server error during tenant resolution." }
            });
            // do NOT call _next(context)
        }
    }

    private static async Task<bool> TryResolveDevTenantAsync(
        ITenantRepository repo,
        ICacheService cache,
        Services.TenantContext tenantCtx,
        HttpContext context)
    {
        const string cacheKey = "tenant:dev-default";

        var cached = await cache.GetAsync<TenantCacheEntry?>(cacheKey);
        if (cached is not null)
        {
            // Same stale-cache guard as the production path: a dev tenant can
            // also be suspended, so re-validate before trusting the cache.
            if (!cached.CanAccess())
            {
                await WriteTenantSuspendedAsync(context);
                return false;
            }

            tenantCtx.TenantId = cached.TenantId;
            return true;
        }

        // Get first active tenant as dev default
        var tenants = await repo.GetAllAsync();
        var devTenant = tenants.FirstOrDefault(t => t.IsActive && t.CanAccess());

        if (devTenant == null)
        {
            tenantCtx.TenantId = Guid.Empty;
            return true; // proceed; downstream code treats Empty as "dev, no tenant"
        }

        var entry = new TenantCacheEntry
        {
            TenantId           = devTenant.Id,
            TenantSlug         = devTenant.Subdomain,
            IsActive           = devTenant.IsActive,
            SubscriptionEndsAt = devTenant.SubscriptionEndsAt,
            TrialEndsAt        = devTenant.TrialEndsAt
        };
        await cache.SetAsync(cacheKey, entry, DevCacheTtl);

        tenantCtx.TenantId = devTenant.Id;
        return true;
    }

    private static async Task WriteTenantSuspendedAsync(HttpContext context)
    {
        context.Response.StatusCode = 403;
        await context.Response.WriteAsJsonAsync(new
        {
            isSuccess = false,
            data = (object?)null,
            error = "TenantSuspended",
            message = "This clinic account is suspended or subscription has expired.",
            errors = new[] { "Tenant is no longer accessible." }
        });
    }

    private static string ExtractSubdomain(string host)
    {
        // clinic1.mediqueue.com → clinic1
        // mediqueue.com → empty (root domain, no tenant)
        var parts = host.Split('.');
        return parts.Length >= 3 ? parts[0] : string.Empty;
    }
}
