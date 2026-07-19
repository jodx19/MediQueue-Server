using System.Threading.Tasks;

namespace MediQueue.Application.Interfaces;

public interface IWhatsAppService
{
    Task SendAsync(string toPhone, string message);

    Task SendAppointmentConfirmationAsync(
        string toPhone, string formattedMessage);

    Task SendAppointmentReminderAsync(
        string toPhone, string formattedMessage);

    Task SendAppointmentCancellationAsync(
        string toPhone, string formattedMessage);

    Task SendStaffNotificationAsync(
        string toPhone, string message);
}
