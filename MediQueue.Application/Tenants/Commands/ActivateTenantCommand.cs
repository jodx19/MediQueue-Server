using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.Tenants.Commands;

public record ActivateTenantCommand(Guid TenantId) : IRequest<Result>;

public class ActivateTenantCommandHandler : IRequestHandler<ActivateTenantCommand, Result>
{
    private readonly ITenantRepository _tenantRepo;
    private readonly IUnitOfWork _unitOfWork;

    public ActivateTenantCommandHandler(ITenantRepository tenantRepo, IUnitOfWork unitOfWork)
    {
        _tenantRepo = tenantRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ActivateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepo.GetByIdAsync(request.TenantId, cancellationToken);

        if (tenant == null)
            return Result.Failure("Tenant not found.");

        tenant.Activate();
        await _tenantRepo.UpdateAsync(tenant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
