// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Interfaces\IRealtimeService.cs
using System;
using System.Threading.Tasks;

namespace MediQueue.Application.Interfaces;

/// <summary>
/// Service for sending real-time updates via SignalR or similar technologies.
/// </summary>
public interface IRealtimeService
{
    Task SendToPatientAsync(Guid patientId, string eventName, object data);
    Task SendToDoctorAsync(Guid doctorId, string eventName, object data);
    Task SendToGroupAsync(string groupName, string eventName, object data);
    Task BroadcastAsync(string eventName, object data);
    Task NotifySlotUpdatedAsync(Guid doctorId, DateOnly date);
    Task NotifyPrescriptionReadyAsync(Guid patientId, Guid visitId);
    Task NotifyAppointmentCancelledAsync(Guid appointmentId, string reason);
}
