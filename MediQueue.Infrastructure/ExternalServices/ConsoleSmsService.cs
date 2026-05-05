// Path: MediQueue.Infrastructure/ExternalServices/ConsoleSmsService.cs
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MediQueue.Application.Interfaces;

namespace MediQueue.Infrastructure.ExternalServices;

/// <summary>
/// Mock SMS service that logs SMS content to the console.
/// Replace with a real provider (e.g., Twilio, Vonage) in production.
/// </summary>
public class ConsoleSmsService : ISmsService
{
    private readonly ILogger<ConsoleSmsService> _logger;

    public ConsoleSmsService(ILogger<ConsoleSmsService> logger)
    {
        _logger = logger;
    }

    public Task SendSmsAsync(string phoneNumber, string message)
    {
        _logger.LogInformation(
            "[SMS Mock] To: {PhoneNumber} | Message: {Message}",
            phoneNumber,
            message);

        return Task.CompletedTask;
    }

    public Task SendAppointmentConfirmationAsync(string patientPhone, string patientName, string doctorName, DateTime scheduledAt)
    {
        var message = $"Confirmation: Dear {patientName}, your appointment with {doctorName} is confirmed for {scheduledAt:f}.";
        return SendSmsAsync(patientPhone, message);
    }

    public Task SendAppointmentReminderAsync(string patientPhone, string patientName, string doctorName, DateTime scheduledAt)
    {
        var message = $"Reminder: Dear {patientName}, you have an appointment with {doctorName} at {scheduledAt:t} today.";
        return SendSmsAsync(patientPhone, message);
    }

    public Task SendAppointmentCancellationAsync(string patientPhone, string reason)
    {
        var message = $"Cancellation: Your appointment has been cancelled. Reason: {reason}.";
        return SendSmsAsync(patientPhone, message);
    }
}
