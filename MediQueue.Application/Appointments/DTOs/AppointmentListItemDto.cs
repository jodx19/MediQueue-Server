using System;

namespace MediQueue.Application.Appointments.DTOs;

/// <summary>Unified list row for today / upcoming / range views.</summary>
public class AppointmentListItemDto
{
    public Guid Id { get; set; }
    public Guid? VisitId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string PatientMrn { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public string DoctorSpecialty { get; set; } = string.Empty;
    public DateTime ScheduledAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string? Reason { get; set; }
}
