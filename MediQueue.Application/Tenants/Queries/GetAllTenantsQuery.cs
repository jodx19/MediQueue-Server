using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.Tenants.Queries;

public record GetAllTenantsQuery() : IRequest<Result<List<TenantDto>>>;

public record TenantDto(
    Guid Id,
    string Name,
    string Subdomain,
    string AdminEmail,
    string Plan,
    bool IsActive,
    DateTime CreatedAt,
    DateTime TrialEndsAt,
    DateTime? SubscriptionEndsAt
);

public class GetAllTenantsQueryHandler : IRequestHandler<GetAllTenantsQuery, Result<List<TenantDto>>>
{
    private readonly ITenantRepository _tenantRepo;

    public GetAllTenantsQueryHandler(ITenantRepository tenantRepo)
    {
        _tenantRepo = tenantRepo;
    }

    public async Task<Result<List<TenantDto>>> Handle(GetAllTenantsQuery request, CancellationToken cancellationToken)
    {
        var tenants = await _tenantRepo.GetAllAsync(cancellationToken);
        
        var dtos = tenants.Select(t => new TenantDto(
            t.Id,
            t.Name,
            t.Subdomain,
            t.AdminEmail,
            t.Plan.ToString(),
            t.IsActive,
            t.CreatedAt,
            t.TrialEndsAt,
            t.SubscriptionEndsAt
        )).ToList();

        return Result<List<TenantDto>>.Success(dtos);
    }
}
