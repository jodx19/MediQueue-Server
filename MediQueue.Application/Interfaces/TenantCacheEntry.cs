using System;

namespace MediQueue.Application.Interfaces;

/// <summary>
/// Cached snapshot of the tenant fields needed by
/// <c>TenantResolutionMiddleware</c> to authorize a request WITHOUT a DB hit.
///
/// IMPORTANT: On every cache hit, callers MUST re-validate <see cref="CanAccess"/>
/// before trusting this entry — the cached value can be up to
/// <c>TenantResolutionMiddleware.CacheTtl</c> minutes old and the tenant may
/// have been suspended or its subscription expired in that window.
/// </summary>
public sealed class TenantCacheEntry
{
    public Guid TenantId { get; set; }
    public string TenantSlug { get; set; } = string.Empty;

    /// <summary>Mirrors <see cref="Domain.Entities.Tenant.IsActive"/>.</summary>
    public bool IsActive { get; set; }

    /// <summary>Mirrors <see cref="Domain.Entities.Tenant.SubscriptionEndsAt"/>.</summary>
    public DateTime? SubscriptionEndsAt { get; set; }

    /// <summary>Mirrors <see cref="Domain.Entities.Tenant.TrialEndsAt"/>.</summary>
    public DateTime TrialEndsAt { get; set; }

    /// <summary>
    /// Same semantics as <see cref="Domain.Entities.Tenant.CanAccess"/>:
    /// active AND (trial still running OR subscription still running).
    /// Computed at request time from cached values so a recently-suspended
    /// tenant is rejected even before the cache entry has been refreshed.
    /// </summary>
    public bool CanAccess()
        => IsActive &&
           (TrialEndsAt > DateTime.UtcNow ||
            (SubscriptionEndsAt.HasValue && SubscriptionEndsAt.Value > DateTime.UtcNow));
}
