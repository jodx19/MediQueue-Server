// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Domain\Entities\Appointment.cs
using System;
using MediQueue.Domain.Common;
using MediQueue.Domain.Enums;
using MediQueue.Domain.Events;
using MediQueue.Domain.Exceptions;

namespace MediQueue.Domain.Entities;

/// <summary>
/// Represents an appointment aggregate root.
/// </summary>
public class Appointment : BaseAggregateRoot
{
    public Guid PatientId { get; private set; }
    public Guid DoctorId { get; private set; }
    public Guid ClinicId { get; private set; }
    public DateTime ScheduledAt { get; private set; }
    public int DurationMinutes { get; private set; }
    public AppointmentStatus Status { get; private set; }
    public AppointmentPriority Priority { get; private set; }
    public VisitType VisitType { get; private set; }
    public string ChiefComplaint { get; private set; }
    public string? Notes { get; private set; }
    public string? CancellationReason { get; private set; }
    public string? RoomNumber { get; private set; }
    public DateTime? ActualStartTime { get; private set; }
    public DateTime? ActualEndTime { get; private set; }
    public byte[] RowVersion { get; private set; }

    private Appointment() 
    { 
        // For EF Core
        ChiefComplaint = null!;
        RowVersion = [];
    }

    private Appointment(
        Guid patientId,
        Guid doctorId,
        Guid clinicId,
        DateTime scheduledAt,
        int durationMinutes,
        AppointmentPriority priority,
        VisitType visitType,
        string chiefComplaint,
        string? notes = null)
    {
        PatientId = patientId;
        DoctorId = doctorId;
        ClinicId = clinicId;
        ScheduledAt = scheduledAt;
        DurationMinutes = durationMinutes;
        Status = AppointmentStatus.Scheduled;
        Priority = priority;
        VisitType = visitType;
        ChiefComplaint = chiefComplaint;
        Notes = notes;
        RowVersion = [];
    }

    /// <summary>
    /// Factory method to book a new appointment.
    /// </summary>
    public static Appointment Book(
        Guid patientId,
        Guid doctorId,
        Guid clinicId,
        DateTime scheduledAt,
        int durationMinutes,
        AppointmentPriority priority,
        VisitType visitType,
        string chiefComplaint,
        string? notes = null)
    {
        var appointment = new Appointment(
            patientId,
            doctorId,
            clinicId,
            scheduledAt,
            durationMinutes,
            priority,
            visitType,
            chiefComplaint,
            notes);

        appointment.AddDomainEvent(new AppointmentBookedEvent(
            appointment.Id,
            patientId,
            doctorId,
            scheduledAt,
            DateTime.UtcNow));

        return appointment;
    }

    /// <summary>
    /// Confirms the appointment.
    /// </summary>
    public void Confirm()
    {
        if (Status != AppointmentStatus.Scheduled)
            throw new InvalidAppointmentStatusException(Status.ToString(), "Confirm");

        Status = AppointmentStatus.Confirmed;
        SetUpdated();

        AddDomainEvent(new AppointmentConfirmedEvent(Id, PatientId, DoctorId, DateTime.UtcNow));
    }

    /// <summary>
    /// Checks in the patient for the appointment.
    /// </summary>
    public void CheckIn()
    {
        if (Status != AppointmentStatus.Confirmed)
            throw new InvalidAppointmentStatusException(Status.ToString(), "CheckIn");

        Status = AppointmentStatus.CheckedIn;
        ActualStartTime = DateTime.UtcNow; // Often checked in means they arrived, actual start is when doctor sees them, but per requirements CheckIn sets ActualStartTime
        SetUpdated();
    }

    /// <summary>
    /// Starts the appointment.
    /// </summary>
    public void Start()
    {
        if (Status != AppointmentStatus.CheckedIn)
            throw new InvalidAppointmentStatusException(Status.ToString(), "Start");

        Status = AppointmentStatus.InProgress;
        SetUpdated();

        AddDomainEvent(new AppointmentStartedEvent(Id, DateTime.UtcNow));
    }

    /// <summary>
    /// Completes the appointment.
    /// </summary>
    public void Complete()
    {
        if (Status != AppointmentStatus.InProgress)
            throw new InvalidAppointmentStatusException(Status.ToString(), "Complete");

        Status = AppointmentStatus.Completed;
        ActualEndTime = DateTime.UtcNow;
        SetUpdated();

        AddDomainEvent(new AppointmentCompletedEvent(Id, DateTime.UtcNow));
    }

    /// <summary>
    /// Cancels the appointment.
    /// </summary>
    public void Cancel(string reason)
    {
        if (Status == AppointmentStatus.Completed || Status == AppointmentStatus.Cancelled)
            throw new InvalidAppointmentStatusException(Status.ToString(), "Cancel");

        Status = AppointmentStatus.Cancelled;
        CancellationReason = reason;
        SetUpdated();

        AddDomainEvent(new AppointmentCancelledEvent(Id, reason, DateTime.UtcNow));
    }

    /// <summary>
    /// Reschedules the appointment.
    /// </summary>
    public void Reschedule(DateTime newDateTime)
    {
        if (Status == AppointmentStatus.Completed || Status == AppointmentStatus.Cancelled)
            throw new InvalidAppointmentStatusException(Status.ToString(), "Reschedule");

        var oldDateTime = ScheduledAt;
        ScheduledAt = newDateTime;
        Status = AppointmentStatus.Scheduled; // Reset to scheduled
        SetUpdated();

        AddDomainEvent(new AppointmentRescheduledEvent(Id, oldDateTime, newDateTime, DateTime.UtcNow));
    }

    /// <summary>
    /// Marks the appointment as a no-show.
    /// </summary>
    public void MarkNoShow()
    {
        if (Status == AppointmentStatus.Completed || Status == AppointmentStatus.Cancelled)
            throw new InvalidAppointmentStatusException(Status.ToString(), "MarkNoShow");

        Status = AppointmentStatus.NoShow;
        SetUpdated();

        AddDomainEvent(new AppointmentNoShowEvent(Id, DateTime.UtcNow));
    }
}
