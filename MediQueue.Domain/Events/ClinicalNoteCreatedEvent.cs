using System;
using MediQueue.Domain.Common;

namespace MediQueue.Domain.Events;

/// <summary>
/// Event raised when a clinical note is created.
/// </summary>
/// <param name="NoteId">The clinical note identifier.</param>
/// <param name="AppointmentId">The appointment identifier.</param>
/// <param name="PatientId">The patient identifier.</param>
/// <param name="OccurredAt">The time when the event occurred.</param>
public sealed record ClinicalNoteCreatedEvent(Guid NoteId, Guid AppointmentId, Guid PatientId, DateTime OccurredAt) : IDomainEvent;
