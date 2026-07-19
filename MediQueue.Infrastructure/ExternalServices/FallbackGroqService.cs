using System.Collections.Generic;
using System.Threading.Tasks;
using MediQueue.Application.Interfaces;

namespace MediQueue.Infrastructure.ExternalServices;

public class FallbackGroqService : IGroqService
{
    public Task<string> GenerateAppointmentConfirmationAsync(
        AppointmentMessageContext ctx)
        => Task.FromResult(
            $"أهلاً {ctx.PatientFirstName} \U0001F60A\n" +
            $"تم تأكيد حجزك في {ctx.ClinicName} \U0001F3E5\n" +
            $"\U0001F468\u200D\u2695\uFE0F {ctx.DoctorName}\n" +
            $"\U0001F4C5 موعدك: {FormatArabicDate(ctx.AppointmentDateTime)}\n\n" +
            $"للتأكيد: رسّل (تأكيد)\n" +
            $"لإعادة الجدولة: رسّل (تأجيل)");

    public Task<string> GenerateAppointmentReminderAsync(
        AppointmentMessageContext ctx)
        => Task.FromResult(
            $"تذكير \U0001F514 أهلاً {ctx.PatientFirstName}\n" +
            $"موعدك في {ctx.ClinicName} بعد ساعتين\n" +
            $"\U0001F468\u200D\u2695\uFE0F {ctx.DoctorName}");

    public Task<string> GenerateAppointmentCancellationAsync(
        AppointmentMessageContext ctx, string reason)
        => Task.FromResult(
            $"عزيزي {ctx.PatientFirstName} \U0001F614\n" +
            $"تم إلغاء موعدك في {ctx.ClinicName}\n" +
            $"السبب: {reason}");

    public Task<string> GenerateAppointmentRescheduleAsync(
        AppointmentMessageContext ctx)
        => Task.FromResult(
            $"أهلاً {ctx.PatientFirstName} \U0001F4C5\n" +
            $"تم تغيير موعدك — الموعد الجديد:\n" +
            $"\U0001F468\u200D\u2695\uFE0F {ctx.DoctorName}\n" +
            $"\U0001F4C5 {FormatArabicDate(ctx.AppointmentDateTime)}");

    public Task<string> DetectIntentAsync(string replyText)
    {
        var text = replyText.Trim().ToLower();
        if (text.Contains("تأكيد") || text.Contains("نعم")
            || text.Contains("اه") || text.Contains("ok"))
            return Task.FromResult("confirm");
        if (text.Contains("تأجيل") || text.Contains("لا")
            || text.Contains("لأ"))
            return Task.FromResult("reschedule");
        return Task.FromResult("unknown");
    }

    public Task<string> GenerateAvailableSlotsMessageAsync(
        string patientFirstName, List<SlotOption> slots)
    {
        var lines = string.Join("\n", slots.ConvertAll(s =>
            $"  {s.Number}. {s.FormattedArabic}"));
        return Task.FromResult(
            $"أهلاً {patientFirstName} \U0001F60A\n" +
            $"اختار موعد من المتاح:\n{lines}\n\n" +
            $"ابعت رقم الاختيار (1 أو 2 أو 3)");
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
}
