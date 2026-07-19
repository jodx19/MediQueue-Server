using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediQueue.Application.Interfaces;

public interface IGroqService
{
    Task<string> GenerateAppointmentConfirmationAsync(
        AppointmentMessageContext context);

    Task<string> GenerateAppointmentReminderAsync(
        AppointmentMessageContext context);

    Task<string> GenerateAppointmentCancellationAsync(
        AppointmentMessageContext context,
        string cancellationReason);

    Task<string> GenerateAppointmentRescheduleAsync(
        AppointmentMessageContext context);

    Task<string> DetectIntentAsync(string patientReplyText);

    Task<string> GenerateAvailableSlotsMessageAsync(
        string patientFirstName,
        List<SlotOption> slots);
}

public record AppointmentMessageContext(
    string PatientFirstName,
    string PatientFullName,
    string DoctorName,
    string ClinicName,
    DateTime AppointmentDateTime,
    string? Specialty = null);

public record SlotOption(
    int Number,
    DateTime DateTime,
    string FormattedArabic);
