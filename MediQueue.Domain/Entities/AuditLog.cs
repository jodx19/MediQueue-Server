using System;

namespace MediQueue.Domain.Entities;

public class AuditLog
{

    public Guid   Id         { get; private set; }
    public Guid   TenantId   { get; private set; }
    public Guid?  UserId     { get; private set; }
    public string UserEmail  { get; private set; } = string.Empty;
    public string UserRole   { get; private set; } = string.Empty;

    public string Action     { get; private set; } = string.Empty;
    public string EntityName { get; private set; } = string.Empty;
    public Guid?  EntityId   { get; private set; }

    public string RequestName { get; private set; } = string.Empty;

    public string? OldValues  { get; private set; }
    public string? NewValues  { get; private set; }

    public string? IpAddress  { get; private set; }
    public string? AdditionalData { get; private set; }

    public bool   Succeeded   { get; private set; }
    public string? ErrorMessage { get; private set; }

    public DateTime CreatedAt { get; private set; }

    private AuditLog() { } // EF Core

    public static AuditLog Create(
        Guid   tenantId,
        Guid?  userId,
        string userEmail,
        string userRole,
        string action,
        string entityName,
        Guid?  entityId,
        string requestName,
        bool   succeeded,
        string? oldValues      = null,
        string? newValues      = null,
        string? ipAddress      = null,
        string? additionalData = null,
        string? errorMessage   = null)
    {
        return new AuditLog
        {
            Id             = Guid.NewGuid(),
            TenantId       = tenantId,
            UserId         = userId,
            UserEmail      = userEmail,
            UserRole       = userRole,
            Action         = action,
            EntityName     = entityName,
            EntityId       = entityId,
            RequestName    = requestName,
            Succeeded      = succeeded,
            OldValues      = oldValues,
            NewValues      = newValues,
            IpAddress      = ipAddress,
            AdditionalData = additionalData,
            ErrorMessage   = errorMessage,
            CreatedAt      = DateTime.UtcNow,
        };
    }
}
