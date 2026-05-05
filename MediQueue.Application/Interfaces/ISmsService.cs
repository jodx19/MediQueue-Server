// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Interfaces\ISmsService.cs
using System;
using System.Threading.Tasks;

namespace MediQueue.Application.Interfaces;

public interface ISmsService
{
    Task SendAppointmentConfirmationAsync(string patientPhone, string patientName, string doctorName, DateTime scheduledAt);
    Task SendAppointmentReminderAsync(string patientPhone, string patientName, string doctorName, DateTime scheduledAt);
    Task SendAppointmentCancellationAsync(string patientPhone, string reason);
    Task SendSmsAsync(string phoneNumber, string message);
}
