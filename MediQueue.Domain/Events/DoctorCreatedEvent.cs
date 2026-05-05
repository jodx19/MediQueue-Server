// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Domain\Events\DoctorCreatedEvent.cs
using System;
using MediQueue.Domain.Common;
using MediQueue.Domain.Enums;

namespace MediQueue.Domain.Events;

/// <summary>
/// Event raised when a new doctor is created.
/// </summary>
public sealed record DoctorCreatedEvent(
    Guid DoctorId,
    string FullName,
    MedicalSpecialty Specialty,
    DateTime OccurredAt) : IDomainEvent;
