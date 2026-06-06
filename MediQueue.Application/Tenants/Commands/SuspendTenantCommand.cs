using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.Tenants.Commands;

public record SuspendTenantCommand(Guid TenantId) : IRequest<Result>;

public class SuspendTenantCommandHandler : IRequestHandler<SuspendTenantCommand, Result>
{
    private readonly ITenantRepository _tenantRepo;
    private readonly IUnitOfWork _unitOfWork;

    public SuspendTenantCommandHandler(ITenantRepository tenantRepo, IUnitOfWork unitOfWork)
    {
        _tenantRepo = tenantRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(SuspendTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepo.GetByIdAsync(request.TenantId, cancellationToken);
        
        if (tenant == null)
            return Result.Failure("Tenant not found.");

        tenant.Suspend();
        await _tenantRepo.UpdateAsync(tenant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
