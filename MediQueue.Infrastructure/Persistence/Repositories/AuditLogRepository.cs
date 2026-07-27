using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediQueue.Domain.Entities;
using MediQueue.Domain.Interfaces;
using MediQueue.Infrastructure.Persistence.Context;

namespace MediQueue.Infrastructure.Persistence.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly ClinicDbContext _context;

    public AuditLogRepository(ClinicDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(AuditLog log, CancellationToken cancellationToken = default)
    {
        await _context.AuditLogs.AddAsync(log, cancellationToken);
    }

    public async Task<IReadOnlyList<AuditLog>> GetPagedAsync(
        Guid tenantId, int page, int pageSize,
        Guid? userId = null,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.AuditLogs
            .AsNoTracking()
            .Where(a => a.TenantId == tenantId);

        if (userId.HasValue)
            query = query.Where(a => a.UserId == userId.Value);

        if (from.HasValue)
            query = query.Where(a => a.Timestamp >= from.Value);

        if (to.HasValue)
            query = query.Where(a => a.Timestamp <= to.Value);

        return await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(
        Guid tenantId,
        Guid? userId = null,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.AuditLogs
            .AsNoTracking()
            .Where(a => a.TenantId == tenantId);

        if (userId.HasValue)
            query = query.Where(a => a.UserId == userId.Value);

        if (from.HasValue)
            query = query.Where(a => a.Timestamp >= from.Value);

        if (to.HasValue)
            query = query.Where(a => a.Timestamp <= to.Value);

        return await query.CountAsync(cancellationToken);
    }
}
