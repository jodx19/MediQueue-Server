using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MediQueue.Application.Interfaces;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace MediQueue.Infrastructure.ExternalServices;

public class TwilioWhatsAppService : IWhatsAppService
{
    private readonly string _accountSid;
    private readonly string _authToken;
    private readonly string _fromNumber;
    private readonly ILogger<TwilioWhatsAppService> _logger;
    private readonly bool _isInitialized;

    public TwilioWhatsAppService(
        IConfiguration configuration,
        ILogger<TwilioWhatsAppService> logger)
    {
        _logger = logger;
        _accountSid   = configuration["Twilio:AccountSid"]   ?? string.Empty;
        _authToken    = configuration["Twilio:AuthToken"]    ?? string.Empty;
        _fromNumber   = configuration["Twilio:WhatsAppFromNumber"]
                        ?? "whatsapp:+14155238886";

        if (!string.IsNullOrEmpty(_accountSid) &&
            !string.IsNullOrEmpty(_authToken)  &&
            !_accountSid.StartsWith("REPLACE_WITH"))
        {
            TwilioClient.Init(_accountSid, _authToken);
            _isInitialized = true;
        }
        else
        {
            _logger.LogWarning(
                "[TwilioWhatsApp] Twilio credentials not configured. " +
                "WhatsApp messages will be logged only.");
            _isInitialized = false;
        }
    }

    public async Task SendAsync(string toPhone, string message)
    {
        var normalizedPhone = NormalizePhone(toPhone);

        if (!_isInitialized)
        {
            _logger.LogInformation(
                "[WhatsApp MOCK] To: {Phone} | Message: {Message}",
                normalizedPhone, message);
            return;
        }

        try
        {
            var msg = await MessageResource.CreateAsync(
                to:   new PhoneNumber($"whatsapp:{normalizedPhone}"),
                from: new PhoneNumber(_fromNumber),
                body: message);

            _logger.LogInformation(
                "[WhatsApp] Sent to {Phone}. SID: {Sid}",
                normalizedPhone, msg.Sid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "[WhatsApp] Failed to send to {Phone}", normalizedPhone);
            throw;
        }
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

    private static string NormalizePhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return phone;
        var cleaned = phone.Trim().Replace(" ", "").Replace("-", "");

        if (cleaned.StartsWith("+")) return cleaned;

        if (cleaned.StartsWith("01") && cleaned.Length == 11)
            return $"+2{cleaned}";

        if (cleaned.StartsWith("20") && cleaned.Length == 12)
            return $"+{cleaned}";

        return $"+2{cleaned}";
    }
}
