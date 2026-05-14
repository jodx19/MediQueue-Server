using MediQueue.Application.Appointments.DTOs;
using MediQueue.Domain.Entities;

namespace MediQueue.Application.Appointments;

internal static class AppointmentListItemFactory
{
    public static AppointmentListItemDto ToListItemDto(this Appointment a, Guid? visitId)
    {
        return new AppointmentListItemDto
        {
            Id = a.Id,
            VisitId = visitId,
            PatientName = a.Patient?.PersonName.FullName ?? string.Empty,
            PatientMrn = a.Patient?.MedicalRecordNumber ?? string.Empty,
            DoctorName = a.Doctor != null ? $"Dr. {a.Doctor.PersonName.FullName}" : string.Empty,
            DoctorSpecialty = a.Doctor?.Specialty.ToString() ?? string.Empty,
            ScheduledAt = a.ScheduledAt,
            Status = a.Status.ToString(),
            Type = a.VisitType.ToString(),
            Priority = a.Priority.ToString(),
            Reason = a.ChiefComplaint,
        };
    }
}
