using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MediQueue.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MediQueue.Infrastructure.ExternalServices;

/// <summary>
/// SMS service implementation using Unifonic (https://www.unifonic.com) — 
/// a leading SMS provider serving Egypt, Saudi Arabia, and the wider Arab world.
/// 
/// Environment variables required (set in appsettings.Production.json or as env vars):
///   Sms__AppSid    — Your Unifonic Application SID
///   Sms__SenderName — Approved sender name (e.g. "MediQueue") 
/// 
/// Fallback: If Sms__AppSid is not configured, the service logs a warning and skips sending
/// (graceful degradation — does NOT throw, so the main flow is not disrupted).
/// </summary>
public class UnifonicSmsService : ISmsService
{
    private readonly HttpClient _httpClient;
    private readonly UnifonicSmsOptions _options;
    private readonly ILogger<UnifonicSmsService> _logger;

    private const string UnifonicBaseUrl = "https://el.unifonic.com/rest/SMS/Messages";

    public UnifonicSmsService(
        HttpClient httpClient,
        IOptions<UnifonicSmsOptions> options,
        ILogger<UnifonicSmsService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendSmsAsync(string phoneNumber, string message)
    {
        if (string.IsNullOrWhiteSpace(_options.AppSid))
        {
            _logger.LogWarning("[SMS] Unifonic AppSid not configured. SMS to {PhoneNumber} was NOT sent. Message: {Message}", phoneNumber, message);
            return;
        }

        // Normalize Egyptian numbers: 01x → +201x
        var normalizedPhone = NormalizeEgyptianPhone(phoneNumber);

        try
        {
            var formContent = new FormUrlEncodedContent(new[]
            {
                new System.Collections.Generic.KeyValuePair<string, string>("AppSid",     _options.AppSid),
                new System.Collections.Generic.KeyValuePair<string, string>("SenderName", _options.SenderName),
                new System.Collections.Generic.KeyValuePair<string, string>("Recipient",  normalizedPhone),
                new System.Collections.Generic.KeyValuePair<string, string>("Body",       message),
                new System.Collections.Generic.KeyValuePair<string, string>("responseType", "JSON"),
                new System.Collections.Generic.KeyValuePair<string, string>("CorrelationID", Guid.NewGuid().ToString())
            });

            var response = await _httpClient.PostAsync(UnifonicBaseUrl, formContent);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("[SMS] Message sent via Unifonic to {PhoneNumber}", normalizedPhone);
            }
            else
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogError("[SMS] Unifonic rejected message to {PhoneNumber}. Status: {Status}. Body: {Body}",
                    normalizedPhone, response.StatusCode, body);
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "[SMS] Network error sending SMS to {PhoneNumber}", normalizedPhone);
        }
    }

    public Task SendAppointmentConfirmationAsync(
        string patientPhone, string patientName, string doctorName, DateTime scheduledAt)
    {
        // Arabic message for Egyptian/Arab market
        var message = $"مرحباً {patientName}، تم تأكيد موعدك مع {doctorName} بتاريخ {scheduledAt:dd/MM/yyyy} الساعة {scheduledAt:HH:mm}. MediQueue";
        return SendSmsAsync(patientPhone, message);
    }

    public Task SendAppointmentReminderAsync(
        string patientPhone, string patientName, string doctorName, DateTime scheduledAt)
    {
        var message = $"تذكير: {patientName}، لديك موعد مع {doctorName} اليوم الساعة {scheduledAt:HH:mm}. MediQueue";
        return SendSmsAsync(patientPhone, message);
    }

    public Task SendAppointmentCancellationAsync(string patientPhone, string reason)
    {
        var message = $"نأسف، تم إلغاء موعدك. السبب: {reason}. للحجز مرة أخرى، تواصل معنا. MediQueue";
        return SendSmsAsync(patientPhone, message);
    }

    /// <summary>
    /// Normalizes Egyptian mobile numbers to international format (+2 prefix).
    /// Examples: 01012345678 → +201012345678 | 201012345678 → +201012345678
    /// </summary>
    private static string NormalizeEgyptianPhone(string phone)
    {
        phone = phone.Trim().Replace(" ", "").Replace("-", "");

        if (phone.StartsWith("+"))
            return phone; // Already international format

        if (phone.StartsWith("00"))
            return "+" + phone[2..]; // 00201... → +201...

        if (phone.StartsWith("2"))
            return "+" + phone; // 201... → +201...

        if (phone.StartsWith("01") || phone.StartsWith("02") || phone.StartsWith("03"))
            return "+2" + phone; // Egyptian numbers: 01x → +201x

        // For non-Egyptian numbers, assume already correct or return as-is with +
        return phone.StartsWith("+") ? phone : "+" + phone;
    }
}

/// <summary>Configuration options for Unifonic SMS service.</summary>
public class UnifonicSmsOptions
{
    public const string SectionName = "Sms";

    /// <summary>Unifonic Application SID — set via env var: Sms__AppSid</summary>
    public string AppSid { get; set; } = string.Empty;

    /// <summary>Approved sender name/number — set via env var: Sms__SenderName</summary>
    public string SenderName { get; set; } = "MediQueue";
}
