using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Application.Interfaces;
using MediQueue.Domain.Enums;
using MediQueue.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace MediQueue.Application.Appointments.Commands;

public class SendAppointmentReminderCommand : ICommand<bool>
{
    public Guid AppointmentId { get; set; }
    public SendAppointmentReminderCommand(Guid appointmentId)
        => AppointmentId = appointmentId;
}

public class SendAppointmentReminderCommandHandler
    : IRequestHandler<SendAppointmentReminderCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISmsService _smsService;
    private readonly IWhatsAppService _whatsAppService;
    private readonly IGroqService _groqService;
    private readonly ISettingsRepository _settingsRepository;
    private readonly ILogger<SendAppointmentReminderCommandHandler> _logger;

    public SendAppointmentReminderCommandHandler(
        IUnitOfWork unitOfWork,
        ISmsService smsService,
        IWhatsAppService whatsAppService,
        IGroqService groqService,
        ISettingsRepository settingsRepository,
        ILogger<SendAppointmentReminderCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _smsService = smsService;
        _whatsAppService = whatsAppService;
        _groqService = groqService;
        _settingsRepository = settingsRepository;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        SendAppointmentReminderCommand request,
        CancellationToken cancellationToken)
    {
        var appointment = await _unitOfWork.Appointments.GetByIdAsync(request.AppointmentId);
        if (appointment == null)
            return Result<bool>.Failure(
                $"Appointment {request.AppointmentId} not found for reminder.");

        if (appointment.Status == AppointmentStatus.Cancelled ||
            appointment.Status == AppointmentStatus.Completed ||
            appointment.Status == AppointmentStatus.NoShow)
        {
            return Result<bool>.Success(true);
        }

        var patient = await _unitOfWork.Patients.GetByIdAsync(appointment.PatientId);
        if (patient == null)
            return Result<bool>.Failure("Patient not found for reminder.");

        var doctor = await _unitOfWork.Doctors.GetByIdAsync(appointment.DoctorId);
        if (doctor == null)
            return Result<bool>.Failure("Doctor not found for reminder.");

        var patientPhone = patient.ContactInfo.Phone
            ?? patient.ContactInfo.AlternativePhone;

        if (string.IsNullOrWhiteSpace(patientPhone))
            return Result<bool>.Failure(
                $"Patient {patient.PersonName.FullName} has no phone number on record.");

        await _smsService.SendAppointmentReminderAsync(
            patientPhone,
            patient.PersonName.FullName,
            doctor.PersonName.FullName,
            appointment.ScheduledAt);

        try
        {
            var settings = await _settingsRepository.GetSettingsAsync(cancellationToken);
            var ctx = new AppointmentMessageContext(
                PatientFirstName: patient.PersonName.FirstName,
                PatientFullName: patient.PersonName.FullName,
                DoctorName: $"د. {doctor.PersonName.FullName}",
                ClinicName: settings?.ClinicName ?? "عيادتنا",
                AppointmentDateTime: appointment.ScheduledAt,
                Specialty: doctor.Specialty.ToString());

            var arabicMessage = await _groqService.GenerateAppointmentReminderAsync(ctx);
            await _whatsAppService.SendAppointmentReminderAsync(patientPhone, arabicMessage);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "WhatsApp reminder failed for appointment {AppointmentId}. SMS reminder was sent successfully.",
                request.AppointmentId);
        }

        return Result<bool>.Success(true);
    }
}
