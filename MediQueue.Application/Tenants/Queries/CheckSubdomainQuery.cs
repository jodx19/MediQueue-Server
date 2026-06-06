using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.Tenants.Queries;

public record CheckSubdomainQuery(string Subdomain) : IRequest<Result<CheckSubdomainResponse>>;

public record CheckSubdomainResponse(bool Available);

public class CheckSubdomainQueryHandler : IRequestHandler<CheckSubdomainQuery, Result<CheckSubdomainResponse>>
{
    private readonly ITenantRepository _tenantRepo;

    public CheckSubdomainQueryHandler(ITenantRepository tenantRepo)
    {
        _tenantRepo = tenantRepo;
    }

    public async Task<Result<CheckSubdomainResponse>> Handle(CheckSubdomainQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Subdomain) || request.Subdomain.Length < 4)
            return Result<CheckSubdomainResponse>.Success(new CheckSubdomainResponse(false));

        var exists = await _tenantRepo.SubdomainExistsAsync(request.Subdomain, cancellationToken);
        
        return Result<CheckSubdomainResponse>.Success(new CheckSubdomainResponse(!exists));
    }
}
