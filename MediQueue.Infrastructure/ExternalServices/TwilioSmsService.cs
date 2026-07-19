using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MediQueue.Application.Interfaces;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace MediQueue.Infrastructure.ExternalServices;

public class TwilioSmsService : ISmsService
{
    private readonly string _accountSid;
    private readonly string _authToken;
    private readonly string _fromNumber;
    private readonly ILogger<TwilioSmsService> _logger;

    public TwilioSmsService(IConfiguration configuration, ILogger<TwilioSmsService> logger)
    {
        _logger = logger;
        _accountSid = configuration["Twilio:AccountSid"] ?? string.Empty;
        _authToken = configuration["Twilio:AuthToken"] ?? string.Empty;
        _fromNumber = configuration["Twilio:FromNumber"] ?? string.Empty;

        if (!string.IsNullOrEmpty(_accountSid) && !string.IsNullOrEmpty(_authToken))
        {
            TwilioClient.Init(_accountSid, _authToken);
        }
    }

    public async Task SendSmsAsync(string phoneNumber, string message)
    {
        _logger.LogInformation("Sending SMS via Twilio to {PhoneNumber}", phoneNumber);

        try
        {
            var messageResource = await MessageResource.CreateAsync(
                to: new PhoneNumber(phoneNumber),
                from: new PhoneNumber(_fromNumber),
                body: message
            );

            _logger.LogInformation("Twilio SMS sent successfully. SID: {Sid}", messageResource.Sid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS via Twilio to {PhoneNumber}", phoneNumber);
            throw;
        }
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
