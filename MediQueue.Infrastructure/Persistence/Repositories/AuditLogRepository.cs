using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediQueue.Domain.Entities;
using MediQueue.Domain.Interfaces;
using MediQueue.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace MediQueue.Infrastructure.Persistence.Repositories;

public sealed class AuditLogRepository : IAuditLogRepository
{
    private readonly ClinicDbContext _context;

    public AuditLogRepository(ClinicDbContext context)
        => _context = context;

    public async Task AddAsync(
        AuditLog log,
        CancellationToken ct = default)
    {
        await _context.AuditLogs.AddAsync(log, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<AuditLog>> GetByTenantAsync(
        Guid      tenantId,
        DateTime? from       = null,
        DateTime? to         = null,
        string?   action     = null,
        string?   entityName = null,
        Guid?     userId     = null,
        int       page       = 1,
        int       pageSize   = 50,
        CancellationToken ct = default)
    {
        // IgnoreQueryFilters لأن AuditLog لا يرث BaseEntity
        // لكن نضيف TenantId filter يدوياً
        var query = _context.AuditLogs
            .AsNoTracking()
            .Where(a => a.TenantId == tenantId);

        if (from.HasValue)
            query = query.Where(a => a.CreatedAt >= from.Value);

        if (to.HasValue)
            query = query.Where(a => a.CreatedAt <= to.Value);

        if (!string.IsNullOrWhiteSpace(action))
            query = query.Where(a => a.Action == action);

        if (!string.IsNullOrWhiteSpace(entityName))
            query = query.Where(a => a.EntityName == entityName);

        if (userId.HasValue)
            query = query.Where(a => a.UserId == userId);

        return await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<int> CountByTenantAsync(
        Guid      tenantId,
        DateTime? from       = null,
        DateTime? to         = null,
        string?   action     = null,
        string?   entityName = null,
        Guid?     userId     = null,
        CancellationToken ct = default)
    {
        var query = _context.AuditLogs
            .AsNoTracking()
            .Where(a => a.TenantId == tenantId);

        if (from.HasValue)
            query = query.Where(a => a.CreatedAt >= from.Value);

        if (to.HasValue)
            query = query.Where(a => a.CreatedAt <= to.Value);

        if (!string.IsNullOrWhiteSpace(action))
            query = query.Where(a => a.Action == action);

        if (!string.IsNullOrWhiteSpace(entityName))
            query = query.Where(a => a.EntityName == entityName);

        if (userId.HasValue)
            query = query.Where(a => a.UserId == userId);

        return await query.CountAsync(ct);
    }

    public async Task<IReadOnlyList<AuditLog>> GetByEntityAsync(
        Guid              entityId,
        string            entityName,
        CancellationToken ct = default)
    {
        return await _context.AuditLogs
            .AsNoTracking()
            .Where(a => a.EntityId   == entityId
                     && a.EntityName == entityName)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(ct);
    }
}
