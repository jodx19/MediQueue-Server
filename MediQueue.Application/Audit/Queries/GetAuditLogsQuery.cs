using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediQueue.Application.Common;
using MediQueue.Application.Common.DTOs;
using MediQueue.Application.Interfaces;
using MediQueue.Domain.Interfaces;
using MediatR;

namespace MediQueue.Application.Audit.Queries;

public record GetAuditLogsQuery(
    DateTime? From       = null,
    DateTime? To         = null,
    string?   Action     = null,
    string?   EntityName = null,
    Guid?     UserId     = null,
    int       Page       = 1,
    int       PageSize   = 50)
    : IRequest<PagedResult<AuditLogDto>>;

public sealed class GetAuditLogsQueryHandler
    : IRequestHandler<GetAuditLogsQuery, PagedResult<AuditLogDto>>
{
    private readonly IAuditLogRepository _auditRepo;
    private readonly ITenantContext      _tenantContext;

    public GetAuditLogsQueryHandler(
        IAuditLogRepository auditRepo,
        ITenantContext      tenantContext)
    {
        _auditRepo     = auditRepo;
        _tenantContext = tenantContext;
    }

    public async Task<PagedResult<AuditLogDto>> Handle(
        GetAuditLogsQuery query,
        CancellationToken ct)
    {
        var tenantId = _tenantContext.TenantId;

        var logsTask  = _auditRepo.GetByTenantAsync(
            tenantId, query.From, query.To,
            query.Action, query.EntityName,
            query.UserId, query.Page, query.PageSize, ct);

        var countTask = _auditRepo.CountByTenantAsync(
            tenantId, query.From, query.To,
            query.Action, query.EntityName,
            query.UserId, ct);

        await Task.WhenAll(logsTask, countTask);

        var items = logsTask.Result.Select(l => new AuditLogDto(
            l.Id, l.TenantId, l.UserId, l.UserEmail, l.UserRole,
            l.Action, l.EntityName, l.EntityId, l.RequestName,
            l.Succeeded, l.OldValues, l.NewValues,
            l.IpAddress, l.ErrorMessage, l.CreatedAt))
            .ToList();

        return PagedResult<AuditLogDto>.Create(
            items, countTask.Result, query.Page, query.PageSize);
    }
}
