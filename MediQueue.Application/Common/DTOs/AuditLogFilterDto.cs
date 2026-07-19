using System;

namespace MediQueue.Application.Common.DTOs;

public record AuditLogFilterDto(
    DateTime? From       = null,
    DateTime? To         = null,
    string?   Action     = null,
    string?   EntityName = null,
    Guid?     UserId     = null,
    int       Page       = 1,
    int       PageSize   = 50);
