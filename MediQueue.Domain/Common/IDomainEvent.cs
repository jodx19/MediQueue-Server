// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Domain\Common\IDomainEvent.cs
using System;

namespace MediQueue.Domain.Common;

/// <summary>
/// Represents a domain event marker interface.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Gets the date and time when the event occurred.
    /// </summary>
    DateTime OccurredAt { get; }
}

