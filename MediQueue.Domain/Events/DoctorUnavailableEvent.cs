// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Domain\Events\DoctorUnavailableEvent.cs
using System;
using MediQueue.Domain.Common;

namespace MediQueue.Domain.Events;

/// <summary>
/// Event raised when a doctor is marked as unavailable.
/// </summary>
public sealed record DoctorUnavailableEvent(
    Guid DoctorId,
    string Reason,
    DateTime OccurredAt) : IDomainEvent;
