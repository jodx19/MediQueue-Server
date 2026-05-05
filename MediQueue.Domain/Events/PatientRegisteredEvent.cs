// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Domain\Events\PatientRegisteredEvent.cs
using System;
using MediQueue.Domain.Common;

namespace MediQueue.Domain.Events;

/// <summary>
/// Event raised when a new patient is registered.
/// </summary>
public sealed record PatientRegisteredEvent(
    Guid PatientId,
    string FullName,
    string MedicalRecordNumber,
    DateTime OccurredAt) : IDomainEvent;
