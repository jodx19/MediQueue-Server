// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Domain\Common\BaseAggregateRoot.cs
using System.Collections.Generic;

namespace MediQueue.Domain.Common;

/// <summary>
/// Represents the base class for all aggregate roots.
/// </summary>
public abstract class BaseAggregateRoot : AuditableEntity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// Gets the domain events associated with the aggregate.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Adds a domain event to the aggregate.
    /// </summary>
    /// <param name="domainEvent">The domain event to add.</param>
    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Clears all domain events from the aggregate.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
