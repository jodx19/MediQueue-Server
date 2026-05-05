// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Domain\Events\PatientDeactivatedEvent.cs
using System;
using MediQueue.Domain.Common;

namespace MediQueue.Domain.Events;

/// <summary>
/// Event raised when a patient is deactivated.
/// </summary>
public sealed record PatientDeactivatedEvent(
    Guid PatientId,
    DateTime OccurredAt) : IDomainEvent;
