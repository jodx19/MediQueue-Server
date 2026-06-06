using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediQueue.Domain.Entities;

namespace MediQueue.Domain.Interfaces;

public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<Tenant?> GetBySubdomainAsync(string subdomain, CancellationToken ct = default);

    Task<bool> SubdomainExistsAsync(string subdomain, CancellationToken ct = default);

    Task AddAsync(Tenant tenant, CancellationToken ct = default);

    Task UpdateAsync(Tenant tenant, CancellationToken ct = default);

    Task<IReadOnlyList<Tenant>> GetAllAsync(CancellationToken ct = default);
}
