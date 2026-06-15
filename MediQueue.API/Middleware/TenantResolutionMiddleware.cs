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
            tenantCtx.TenantId = await GetDevTenantIdAsync(
                tenantRepository, cacheService);
            tenantCtx.Subdomain = "dev";
            await _next(context);
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
        Tenant? tenant = null;

        try
        {
            // Try cache first
            var cached = await cacheService
                .GetAsync<Guid?>(cacheKey);

            if (cached.HasValue && cached.Value != Guid.Empty)
            {
                tenantCtx.TenantId = cached.Value;
                tenantCtx.Subdomain = subdomain;
            }
            else
            {
                // Lookup from DB
                tenant = await tenantRepository
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

                // Cache for 5 minutes
                await cacheService.SetAsync(
                    cacheKey, tenant.Id,
                    TimeSpan.FromMinutes(5));

                tenantCtx.TenantId = tenant.Id;
                tenantCtx.Subdomain = subdomain;
            }
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
            return; // do NOT call _next(context)
        }
    }

    private static string ExtractSubdomain(string host)
    {
        // clinic1.mediqueue.com → clinic1
        // mediqueue.com → empty (root domain, no tenant)
        var parts = host.Split('.');
        return parts.Length >= 3 ? parts[0] : string.Empty;
    }

    private static async Task<Guid> GetDevTenantIdAsync(
        ITenantRepository repo,
        ICacheService cache)
    {
        const string cacheKey = "tenant:dev-default";

        var cached = await cache.GetAsync<Guid?>(cacheKey);
        if (cached.HasValue && cached.Value != Guid.Empty)
            return cached.Value;

        // Get first active tenant as dev default
        var tenants = await repo.GetAllAsync();
        var devTenant = tenants.FirstOrDefault(t => t.IsActive);

        if (devTenant == null)
            return Guid.Empty;

        await cache.SetAsync(cacheKey, devTenant.Id,
            TimeSpan.FromMinutes(10));

        return devTenant.Id;
    }
}
