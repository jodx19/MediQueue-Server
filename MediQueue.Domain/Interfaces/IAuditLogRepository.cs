using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediQueue.Domain.Entities;

namespace MediQueue.Domain.Interfaces;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog log, CancellationToken ct = default);

    Task<IReadOnlyList<AuditLog>> GetByTenantAsync(
        Guid      tenantId,
        DateTime? from      = null,
        DateTime? to        = null,
        string?   action    = null,
        string?   entityName = null,
        Guid?     userId    = null,
        int       page      = 1,
        int       pageSize  = 50,
        CancellationToken ct = default);

    Task<int> CountByTenantAsync(
        Guid      tenantId,
        DateTime? from      = null,
        DateTime? to        = null,
        string?   action    = null,
        string?   entityName = null,
        Guid?     userId    = null,
        CancellationToken ct = default);

    Task<IReadOnlyList<AuditLog>> GetByEntityAsync(
        Guid   entityId,
        string entityName,
        CancellationToken ct = default);
}
