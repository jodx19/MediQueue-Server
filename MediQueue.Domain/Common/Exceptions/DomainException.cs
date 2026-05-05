namespace MediQueue.Domain.Common.Exceptions;

/// <summary>
/// Base domain exception for business rule violations
/// </summary>
public abstract class DomainException : Exception
{
    public string ErrorCode { get; }
    public string? Details { get; }

    protected DomainException(string errorCode, string message, string? details = null) 
        : base(message)
    {
        ErrorCode = errorCode;
        Details = details;
    }

    protected DomainException(string errorCode, string message, Exception innerException, string? details = null) 
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        Details = details;
    }
}

/// <summary>
/// Exception thrown when a business rule is violated
/// </summary>
public class BusinessRuleViolationException : DomainException
{
    public BusinessRuleViolationException(string ruleName, string message, string? details = null)
        : base($"BUSINESS_RULE_VIOLATION_{ruleName.ToUpper()}", message, details)
    {
    }
}

/// <summary>
/// Exception thrown when an entity is not found
/// </summary>
public class NotFoundException : DomainException
{
    public string EntityType { get; }
    public object EntityId { get; }

    public NotFoundException(string entityType, object entityId)
        : base("ENTITY_NOT_FOUND", $"{entityType} with ID '{entityId}' was not found.")
    {
        EntityType = entityType;
        EntityId = entityId;
    }

    public NotFoundException(string entityType, object entityId, string message)
        : base("ENTITY_NOT_FOUND", message)
    {
        EntityType = entityType;
        EntityId = entityId;
    }
}

/// <summary>
/// Exception thrown when an operation is not allowed
/// </summary>
public class ForbiddenException : DomainException
{
    public ForbiddenException(string operation, string reason)
        : base("OPERATION_FORBIDDEN", $"Operation '{operation}' is not allowed: {reason}")
    {
    }
}

/// <summary>
/// Exception thrown when a resource conflict occurs
/// </summary>
public class ConflictException : DomainException
{
    public string ResourceType { get; }
    public object? ResourceId { get; }

    public ConflictException(string resourceType, object? resourceId, string message)
        : base("RESOURCE_CONFLICT", message)
    {
        ResourceType = resourceType;
        ResourceId = resourceId;
    }
}

/// <summary>
/// Exception thrown when the system is in an invalid state
/// </summary>
public class InvalidStateException : DomainException
{
    public InvalidStateException(string state, string message)
        : base("INVALID_STATE", $"System is in invalid state '{state}': {message}")
    {
    }
}
