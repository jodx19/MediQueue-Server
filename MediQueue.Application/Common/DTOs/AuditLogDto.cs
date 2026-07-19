using System;

namespace MediQueue.Application.Common.DTOs;

public record AuditLogDto(
    Guid      Id,
    Guid      TenantId,
    Guid?     UserId,
    string    UserEmail,
    string    UserRole,
    string    Action,
    string    EntityName,
    Guid?     EntityId,
    string    RequestName,
    bool      Succeeded,
    string?   OldValues,
    string?   NewValues,
    string?   IpAddress,
    string?   ErrorMessage,
    DateTime  CreatedAt);
