// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Appointments\DTOs\AppointmentDtos.cs
using System;
using MediQueue.Domain.Enums;

namespace MediQueue.Application.Appointments.DTOs;

public class AppointmentDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public Guid DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public Guid ClinicId { get; set; }
    public DateTime ScheduledAt { get; set; }
    public int DurationMinutes { get; set; }
    public AppointmentStatus Status { get; set; }
    public AppointmentPriority Priority { get; set; }
    public VisitType VisitType { get; set; }
}

public class AppointmentDetailDto : AppointmentDto
{
    public string ChiefComplaint { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? CancellationReason { get; set; }
    public string? RoomNumber { get; set; }
    public DateTime? ActualStartTime { get; set; }
    public DateTime? ActualEndTime { get; set; }
}

public class AppointmentScheduleItemDto
{
    public Guid AppointmentId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public DateTime ScheduledAt { get; set; }
    public int DurationMinutes { get; set; }
    public AppointmentStatus Status { get; set; }
    public string ChiefComplaint { get; set; } = string.Empty;
}

public class BookAppointmentDto
{
    public Guid PatientId { get; set; }
    public Guid DoctorId { get; set; }
    public DateTime ScheduledAt { get; set; }
    public int DurationMinutes { get; set; }
    public VisitType VisitType { get; set; }
    public AppointmentPriority Priority { get; set; }
    public string ChiefComplaint { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? RoomNumber { get; set; }
}
