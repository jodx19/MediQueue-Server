using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MediQueue.Application.Interfaces;

namespace MediQueue.Infrastructure.ExternalServices;

public class ConsoleWhatsAppService : IWhatsAppService
{
    private readonly ILogger<ConsoleWhatsAppService> _logger;

    public ConsoleWhatsAppService(
        ILogger<ConsoleWhatsAppService> logger)
        => _logger = logger;

    public Task SendAsync(string toPhone, string message)
    {
        _logger.LogInformation(
            "[WhatsApp CONSOLE] ──────────────────────\n" +
            "To:      {Phone}\n" +
            "Message: {Message}\n" +
            "──────────────────────────────────────────",
            toPhone, message);
        return Task.CompletedTask;
    }

    public Task SendAppointmentConfirmationAsync(
        string toPhone, string formattedMessage)
        => SendAsync(toPhone, formattedMessage);

    public Task SendAppointmentReminderAsync(
        string toPhone, string formattedMessage)
        => SendAsync(toPhone, formattedMessage);

    public Task SendAppointmentCancellationAsync(
        string toPhone, string formattedMessage)
        => SendAsync(toPhone, formattedMessage);

    public Task SendStaffNotificationAsync(
        string toPhone, string message)
        => SendAsync(toPhone, message);
}
