using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MediQueue.Application.Interfaces;

namespace MediQueue.Infrastructure.ExternalServices;

public class GroqLlmService : IGroqService
{
    private readonly HttpClient _http;
    private readonly string _model;
    private readonly ILogger<GroqLlmService> _logger;

    private const string SystemPrompt =
        "أنت مساعد ذكي لنظام إدارة عيادات طبية. " +
        "مهمتك كتابة رسائل واتساب مختصرة وودية بالعربية العامية المصرية. " +
        "القواعد الصارمة: " +
        "1. استخدم فقط المعلومات المُعطاة لك — لا تُضف أي معلومات إضافية. " +
        "2. الرسالة يجب أن تكون قصيرة (5-8 أسطر بحد أقصى). " +
        "3. استخدم emoji مناسب بشكل معتدل (2-3 فقط). " +
        "4. أنهِ كل رسالة تأكيد بخيارَي الرد: " +
        "   للتأكيد: رسّل (تأكيد) " +
        "   لإعادة الجدولة: رسّل (تأجيل). " +
        "5. لا تكتب أي نص خارج رسالة الواتساب المطلوبة.";

    public GroqLlmService(
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        ILogger<GroqLlmService> logger)
    {
        _logger = logger;
        _model  = configuration["Groq:Model"]
                  ?? "llama-3.1-8b-instant";

        _http = httpClientFactory.CreateClient("Groq");
        var apiKey = configuration["Groq:ApiKey"] ?? string.Empty;
        _http.DefaultRequestHeaders.Add(
            "Authorization", $"Bearer {apiKey}");
    }

    public async Task<string> GenerateAppointmentConfirmationAsync(
        AppointmentMessageContext ctx)
    {
        var userPrompt =
            $"اكتب رسالة واتساب لتأكيد موعد طبي للمريض:\n" +
            $"اسم المريض: {ctx.PatientFirstName}\n" +
            $"اسم الدكتور: {ctx.DoctorName}\n" +
            $"التخصص: {ctx.Specialty ?? "طب عام"}\n" +
            $"اسم العيادة: {ctx.ClinicName}\n" +
            $"تاريخ ووقت الموعد: " +
            $"{FormatArabicDate(ctx.AppointmentDateTime)}";

        return await CallGroqAsync(userPrompt)
               ?? FallbackConfirmation(ctx);
    }

    public async Task<string> GenerateAppointmentReminderAsync(
        AppointmentMessageContext ctx)
    {
        var userPrompt =
            $"اكتب رسالة واتساب كتذكير بموعد طبي قادم:\n" +
            $"اسم المريض: {ctx.PatientFirstName}\n" +
            $"اسم الدكتور: {ctx.DoctorName}\n" +
            $"اسم العيادة: {ctx.ClinicName}\n" +
            $"الموعد بعد ساعتين تقريباً: " +
            $"{FormatArabicDate(ctx.AppointmentDateTime)}";

        return await CallGroqAsync(userPrompt)
               ?? FallbackReminder(ctx);
    }

    public async Task<string> GenerateAppointmentCancellationAsync(
        AppointmentMessageContext ctx, string reason)
    {
        var userPrompt =
            $"اكتب رسالة واتساب لإخبار المريض بإلغاء موعده:\n" +
            $"اسم المريض: {ctx.PatientFirstName}\n" +
            $"اسم الدكتور: {ctx.DoctorName}\n" +
            $"اسم العيادة: {ctx.ClinicName}\n" +
            $"سبب الإلغاء: {reason}\n" +
            $"الموعد الملغي: {FormatArabicDate(ctx.AppointmentDateTime)}";

        return await CallGroqAsync(userPrompt)
               ?? FallbackCancellation(ctx, reason);
    }

    public async Task<string> GenerateAppointmentRescheduleAsync(
        AppointmentMessageContext ctx)
    {
        var userPrompt =
            $"اكتب رسالة واتساب لإخبار المريض بتغيير موعده:\n" +
            $"اسم المريض: {ctx.PatientFirstName}\n" +
            $"اسم الدكتور: {ctx.DoctorName}\n" +
            $"اسم العيادة: {ctx.ClinicName}\n" +
            $"الموعد الجديد: " +
            $"{FormatArabicDate(ctx.AppointmentDateTime)}";

        return await CallGroqAsync(userPrompt)
               ?? FallbackReschedule(ctx);
    }

    public async Task<string> DetectIntentAsync(string replyText)
    {
        var text = replyText.Trim().ToLower();

        if (text is "تأكيد" or "اوك" or "ok" or "okay"
                 or "نعم" or "آه" or "اه" or "ايوه"
                 or "تمام" or "موافق" or "confirmed")
            return "confirm";

        if (text is "تأجيل" or "لأ" or "لا" or "مش هينفع"
                 or "cancel" or "إلغاء" or "الغاء")
            return "reschedule";

        if (text is "1" or "٢" or "2" or "٣" or "3")
            return $"slot_{text.Replace("٢", "2").Replace("٣", "3")}";

        var prompt =
            $"صنّف نية المريض في الرسالة التالية إلى إحدى الكلمات:\n" +
            $"confirm / reschedule / slot_1 / slot_2 / slot_3 / unknown\n\n" +
            $"رسالة المريض: \"{replyText}\"\n\n" +
            $"أجب بكلمة واحدة فقط بدون أي نص آخر.";

        var result = await CallGroqAsync(prompt);
        return result?.Trim().ToLower() ?? "unknown";
    }

    public Task<string> GenerateAvailableSlotsMessageAsync(
        string patientFirstName, List<SlotOption> slots)
    {
        var slotsText = string.Join("\n",
            slots.ConvertAll(s =>
                $"  {s.Number}\uFE0F\u20E3 {s.FormattedArabic}"));

        return Task.FromResult(
            $"أهلاً {patientFirstName} \U0001F60A\n" +
            $"مواعيد متاحة:\n\n" +
            $"{slotsText}\n\n" +
            $"ابعت رقم الاختيار (1 أو 2 أو 3) \u2709\uFE0F");
    }

    private async Task<string?> CallGroqAsync(string userPrompt)
    {
        try
        {
            var request = new
            {
                model = _model,
                temperature = 0.2,
                max_tokens = 300,
                messages = new[]
                {
                    new { role = "system", content = SystemPrompt },
                    new { role = "user",   content = userPrompt }
                }
            };

            var response = await _http.PostAsJsonAsync(
                "chat/completions", request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "[Groq] API returned {Status}. Using fallback message.",
                    response.StatusCode);
                return null;
            }

            var json = await response.Content
                .ReadFromJsonAsync<JsonElement>();

            return json
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "[Groq] API call failed. Using fallback.");
            return null;
        }
    }

    private static string FormatArabicDate(DateTime dt)
    {
        var days = new[]
        {
            "الأحد", "الاثنين", "الثلاثاء",
            "الأربعاء", "الخميس", "الجمعة", "السبت"
        };
        var months = new[]
        {
            "", "يناير", "فبراير", "مارس", "إبريل", "مايو",
            "يونيو", "يوليو", "أغسطس", "سبتمبر",
            "أكتوبر", "نوفمبر", "ديسمبر"
        };

        var dayName = days[(int)dt.DayOfWeek];
        var hour    = dt.Hour > 12 ? dt.Hour - 12 : dt.Hour;
        var amPm    = dt.Hour >= 12 ? "مساءً" : "صباحاً";
        var minute  = dt.Minute > 0 ? $":{dt.Minute:D2}" : "";

        return $"{dayName} {dt.Day} {months[dt.Month]} " +
               $"الساعة {hour}{minute} {amPm}";
    }

    private static string FallbackConfirmation(
        AppointmentMessageContext ctx) =>
        $"أهلاً {ctx.PatientFirstName} \U0001F60A\n" +
        $"تم تأكيد حجزك في {ctx.ClinicName} \U0001F3E5\n" +
        $"\U0001F468\u200D\u2695\uFE0F {ctx.DoctorName}\n" +
        $"\U0001F4C5 {FormatArabicDate(ctx.AppointmentDateTime)}\n\n" +
        $"للتأكيد: رسّل (تأكيد)\n" +
        $"لإعادة الجدولة: رسّل (تأجيل)";

    private static string FallbackReminder(
        AppointmentMessageContext ctx) =>
        $"تذكير \U0001F514\n" +
        $"أهلاً {ctx.PatientFirstName}، " +
        $"موعدك في {ctx.ClinicName} بعد ساعتين\n" +
        $"\U0001F468\u200D\u2695\uFE0F {ctx.DoctorName}\n" +
        $"\u23F0 {FormatArabicDate(ctx.AppointmentDateTime)}";

    private static string FallbackCancellation(
        AppointmentMessageContext ctx, string reason) =>
        $"عزيزي {ctx.PatientFirstName} \U0001F614\n" +
        $"للأسف تم إلغاء موعدك في {ctx.ClinicName}\n" +
        $"السبب: {reason}\n" +
        $"يرجى التواصل معنا لحجز موعد جديد.";

    private static string FallbackReschedule(
        AppointmentMessageContext ctx) =>
        $"أهلاً {ctx.PatientFirstName} \U0001F4C5\n" +
        $"تم تغيير موعدك في {ctx.ClinicName}\n" +
        $"\U0001F468\u200D\u2695\uFE0F {ctx.DoctorName}\n" +
        $"الموعد الجديد: " +
        $"{FormatArabicDate(ctx.AppointmentDateTime)}";
}
