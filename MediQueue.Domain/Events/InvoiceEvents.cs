// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Domain\Events\InvoiceEvents.cs
using System;
using MediQueue.Domain.Common;
using MediQueue.Domain.Enums;
using MediQueue.Domain.ValueObjects;

namespace MediQueue.Domain.Events;

public sealed record InvoiceCreatedEvent(
    Guid InvoiceId,
    Guid PatientId,
    Money TotalAmount,
    DateTime OccurredAt) : IDomainEvent;

public sealed record PaymentRecordedEvent(
    Guid InvoiceId,
    Money Amount,
    PaymentMethod Method,
    DateTime OccurredAt) : IDomainEvent;

public sealed record InvoicePaidEvent(
    Guid InvoiceId,
    Guid PatientId,
    DateTime OccurredAt) : IDomainEvent;
