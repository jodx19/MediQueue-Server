// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Domain\Events\ClinicalVisitEvents.cs
using System;
using MediQueue.Domain.Common;

namespace MediQueue.Domain.Events;

public sealed record ClinicalVisitCreatedEvent(
    Guid VisitId,
    Guid AppointmentId,
    Guid PatientId,
    DateTime OccurredAt) : IDomainEvent;

public sealed record PrescriptionCreatedEvent(
    Guid PrescriptionId,
    Guid PatientId,
    Guid VisitId,
    DateTime OccurredAt) : IDomainEvent;

public sealed record VisitFinalizedEvent(
    Guid VisitId,
    Guid PatientId,
    Guid DoctorId,
    DateTime OccurredAt) : IDomainEvent;
