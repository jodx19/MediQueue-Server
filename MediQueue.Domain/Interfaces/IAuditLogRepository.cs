using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediQueue.Domain.Entities;

namespace MediQueue.Domain.Interfaces;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog log, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditLog>> GetPagedAsync(Guid tenantId, int page, int pageSize, Guid? userId = null, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid tenantId, Guid? userId = null, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default);
}
