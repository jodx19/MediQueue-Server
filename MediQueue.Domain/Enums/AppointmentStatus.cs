// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Domain\Enums\AppointmentStatus.cs
namespace MediQueue.Domain.Enums;

/// <summary>
/// Represents the status of an appointment.
/// </summary>
public enum AppointmentStatus
{
    Scheduled = 1,
    Confirmed = 2,
    CheckedIn = 3,
    InProgress = 4,
    Completed = 5,
    Cancelled = 6,
    NoShow = 7,
    Rescheduled = 8
}
