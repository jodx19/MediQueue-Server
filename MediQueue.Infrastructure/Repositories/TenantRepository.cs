using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediQueue.Domain.Entities;
using MediQueue.Domain.Interfaces;
using MediQueue.Infrastructure.Persistence.Context;

namespace MediQueue.Infrastructure.Repositories;

public class TenantRepository : ITenantRepository
{
    private readonly ClinicDbContext _context;

    public TenantRepository(ClinicDbContext context)
        => _context = context;

    public async Task<Tenant?> GetByIdAsync(
        Guid id, CancellationToken ct = default)
        => await _context.Tenants
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<Tenant?> GetBySubdomainAsync(
        string subdomain, CancellationToken ct = default)
        => await _context.Tenants
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                t => t.Subdomain == subdomain.ToLowerInvariant(),
                ct);

    public async Task<bool> SubdomainExistsAsync(
        string subdomain, CancellationToken ct = default)
        => await _context.Tenants
            .IgnoreQueryFilters()
            .AnyAsync(
                t => t.Subdomain == subdomain.ToLowerInvariant(),
                ct);

    public async Task AddAsync(
        Tenant tenant, CancellationToken ct = default)
        => await _context.Tenants.AddAsync(tenant, ct);

    public Task UpdateAsync(
        Tenant tenant, CancellationToken ct = default)
    {
        _context.Tenants.Update(tenant);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<Tenant>> GetAllAsync(
        CancellationToken ct = default)
        => await _context.Tenants
            .IgnoreQueryFilters()
            .AsNoTracking()
            .ToListAsync(ct);
}
