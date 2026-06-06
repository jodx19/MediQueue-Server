using System;

namespace MediQueue.Application.Settings.Dtos;

public record ClinicSettingsDto(
    Guid Id,
    string ClinicName,
    string ClinicPhone,
    string ClinicEmail,
    string ClinicAddress,
    string LogoUrl,
    string WorkStartTime,
    string WorkEndTime,
    int AppointmentDurationMinutes,
    string Currency,
    string TimeZone,
    bool AllowOnlineBooking,
    bool RequireDepositForBooking,
    decimal DepositAmount
);
