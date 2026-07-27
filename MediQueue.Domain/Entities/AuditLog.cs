using System;

namespace MediQueue.Domain.Entities;

/// <summary>
/// Represents a single audit log entry. Intentionally does NOT inherit from BaseEntity
/// so it is excluded from the global Multi-Tenancy / SoftDelete query filters and
/// is never accidentally hidden or stamped with the wrong TenantId.
/// </summary>
public class AuditLog
{
    public Guid Id { get; private set; }

    /// <summary>TenantId of the user who performed the action.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>UserId (AppUser.Id) of the actor. Null for unauthenticated operations.</summary>
    public Guid? UserId { get; private set; }

    /// <summary>Email of the actor at time of action.</summary>
    public string? UserEmail { get; private set; }

    /// <summary>Role of the actor (Admin, Doctor, Receptionist, etc.).</summary>
    public string? UserRole { get; private set; }

    /// <summary>The MediatR Command type name, e.g. "CreateAppointmentCommand".</summary>
    public string Action { get; private set; }

    /// <summary>UTC timestamp of when the action was performed.</summary>
    public DateTime Timestamp { get; private set; }

    /// <summary>Whether the command completed successfully.</summary>
    public bool IsSuccess { get; private set; }

    /// <summary>Error message if the command failed.</summary>
    public string? ErrorMessage { get; private set; }

    private AuditLog() 
    { 
        Action = null!;
    }

    private AuditLog(
        Guid tenantId,
        Guid? userId,
        string? userEmail,
        string? userRole,
        string action,
        bool isSuccess,
        string? errorMessage)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        UserId = userId;
        UserEmail = userEmail;
        UserRole = userRole;
        Action = action;
        Timestamp = DateTime.UtcNow;
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    public static AuditLog Create(
        Guid tenantId,
        Guid? userId,
        string? userEmail,
        string? userRole,
        string action,
        bool isSuccess,
        string? errorMessage = null)
    {
        return new AuditLog(tenantId, userId, userEmail, userRole, action, isSuccess, errorMessage);
    }
}
