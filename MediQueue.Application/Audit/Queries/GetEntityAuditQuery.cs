using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediQueue.Application.Common.DTOs;
using MediQueue.Domain.Interfaces;
using MediatR;

namespace MediQueue.Application.Audit.Queries;

public record GetEntityAuditQuery(
    Guid   EntityId,
    string EntityName)
    : IRequest<IReadOnlyList<AuditLogDto>>;

public sealed class GetEntityAuditQueryHandler
    : IRequestHandler<GetEntityAuditQuery, IReadOnlyList<AuditLogDto>>
{
    private readonly IAuditLogRepository _auditRepo;

    public GetEntityAuditQueryHandler(IAuditLogRepository auditRepo)
        => _auditRepo = auditRepo;

    public async Task<IReadOnlyList<AuditLogDto>> Handle(
        GetEntityAuditQuery query,
        CancellationToken   ct)
    {
        var logs = await _auditRepo.GetByEntityAsync(
            query.EntityId, query.EntityName, ct);

        return logs.Select(l => new AuditLogDto(
            l.Id, l.TenantId, l.UserId, l.UserEmail, l.UserRole,
            l.Action, l.EntityName, l.EntityId, l.RequestName,
            l.Succeeded, l.OldValues, l.NewValues,
            l.IpAddress, l.ErrorMessage, l.CreatedAt))
            .ToList();
    }
}
