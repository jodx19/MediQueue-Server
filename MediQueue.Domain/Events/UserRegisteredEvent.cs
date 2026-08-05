// MediQueue.Domain/Events/UserRegisteredEvent.cs
using System;
using MediQueue.Domain.Common;

namespace MediQueue.Domain.Events;

/// <summary>
/// Raised after a new AppUser is successfully persisted.
/// Consumed by the email-verification event handler.
/// </summary>
public class UserRegisteredEvent : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;

    public string UserId { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string VerificationToken { get; init; } = null!;
    public Guid TenantId { get; init; }
}
