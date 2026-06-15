using System;

namespace MediQueue.Application.Interfaces;

/// <summary>
/// Provides the current tenant's identity for this request.
/// Populated by TenantResolutionMiddleware before any handler runs.
/// </summary>
public interface ITenantContext
{
    /// <summary>Current tenant's ID. Guid.Empty if not resolved.</summary>
    Guid TenantId { get; }

    /// <summary>Current tenant's subdomain (e.g. "clinic1")</summary>
    string Subdomain { get; }
}
