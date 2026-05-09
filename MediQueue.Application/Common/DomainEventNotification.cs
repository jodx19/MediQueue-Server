using MediatR;
using MediQueue.Domain.Common;

namespace MediQueue.Application.Common;

/// <summary>
/// A wrapper to allow Domain Events (which are pure C#) to be published via MediatR.
/// </summary>
/// <typeparam name="TDomainEvent">The type of the domain event.</typeparam>
public record DomainEventNotification<TDomainEvent>(TDomainEvent DomainEvent) : INotification 
    where TDomainEvent : IDomainEvent;
