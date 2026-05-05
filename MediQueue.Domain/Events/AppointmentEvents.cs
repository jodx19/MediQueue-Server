// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Domain\Events\AppointmentEvents.cs
using System;
using MediQueue.Domain.Common;

namespace MediQueue.Domain.Events;

public sealed record AppointmentBookedEvent(
    Guid AppointmentId,
    Guid PatientId,
    Guid DoctorId,
    DateTime ScheduledAt,
    DateTime OccurredAt) : IDomainEvent;

public sealed record AppointmentConfirmedEvent(
    Guid AppointmentId,
    Guid PatientId,
    Guid DoctorId,
    DateTime OccurredAt) : IDomainEvent;

public sealed record AppointmentStartedEvent(
    Guid AppointmentId,
    DateTime OccurredAt) : IDomainEvent;

public sealed record AppointmentCompletedEvent(
    Guid AppointmentId,
    DateTime OccurredAt) : IDomainEvent;

public sealed record AppointmentCancelledEvent(
    Guid AppointmentId,
    string Reason,
    DateTime OccurredAt) : IDomainEvent;

public sealed record AppointmentRescheduledEvent(
    Guid AppointmentId,
    DateTime OldDateTime,
    DateTime NewDateTime,
    DateTime OccurredAt) : IDomainEvent;

public sealed record AppointmentNoShowEvent(
    Guid AppointmentId,
    DateTime OccurredAt) : IDomainEvent;
