using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Application.Interfaces;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.AuditLogs.Queries;

public class AuditLogDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string? UserRole { get; set; }
    public string Action { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
}

public class GetAuditLogsQuery : IQuery<PagedResult<AuditLogDto>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public Guid? UserId { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}

public class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, Result<PagedResult<AuditLogDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetAuditLogsQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<Result<PagedResult<AuditLogDto>>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        var items = await _unitOfWork.AuditLogs.GetPagedAsync(
            tenantId, 
            request.Page, 
            request.PageSize, 
            request.UserId, 
            request.From, 
            request.To, 
            cancellationToken);

        var count = await _unitOfWork.AuditLogs.CountAsync(
            tenantId, 
            request.UserId, 
            request.From, 
            request.To, 
            cancellationToken);

        var dtos = items.Select(a => new AuditLogDto
        {
            Id = a.Id,
            UserId = a.UserId,
            UserEmail = a.UserEmail,
            UserRole = a.UserRole,
            Action = a.Action,
            Timestamp = a.Timestamp,
            IsSuccess = a.IsSuccess,
            ErrorMessage = a.ErrorMessage
        }).ToList();

        var pagedResult = PagedResult<AuditLogDto>.Create(dtos, count, request.Page, request.PageSize);
        return Result<PagedResult<AuditLogDto>>.Success(pagedResult);
    }
}
