using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MediQueue.Application.Interfaces;
using MediQueue.Application.Settings.Dtos;
using MediQueue.Domain.Events;
using MediQueue.Domain.Interfaces;

using MediQueue.Application.Common;
using Microsoft.Extensions.Logging;

namespace MediQueue.Application.Appointments.EventHandlers;

public class AppointmentBookedEventHandler : INotificationHandler<DomainEventNotification<AppointmentBookedEvent>>
{
    private readonly ISmsService _smsService;
    private readonly IWhatsAppService _whatsAppService;
    private readonly IGroqService _groqService;
    private readonly ISchedulerService _schedulerService;
    private readonly IRealtimeService _realtimeService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISettingsRepository _settingsRepository;
    private readonly ILogger<AppointmentBookedEventHandler> _logger;

    public AppointmentBookedEventHandler(
        ISmsService smsService,
        IWhatsAppService whatsAppService,
        IGroqService groqService,
        ISchedulerService schedulerService,
        IRealtimeService realtimeService,
        IUnitOfWork unitOfWork,
        ISettingsRepository settingsRepository,
        ILogger<AppointmentBookedEventHandler> logger)
    {
        _smsService = smsService;
        _whatsAppService = whatsAppService;
        _groqService = groqService;
        _schedulerService = schedulerService;
        _realtimeService = realtimeService;
        _unitOfWork = unitOfWork;
        _settingsRepository = settingsRepository;
        _logger = logger;
    }

    public async Task Handle(DomainEventNotification<AppointmentBookedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        var patient = await _unitOfWork.Patients.GetByIdAsync(domainEvent.PatientId);
        var doctor = await _unitOfWork.Doctors.GetByIdAsync(domainEvent.DoctorId);

        if (patient != null && doctor != null)
        {
            var patientName = patient.PersonName.FullName;
            var patientPhone = patient.ContactInfo.Phone;
            var doctorName = doctor.PersonName.FullName;

            await _smsService.SendAppointmentConfirmationAsync(patientPhone, patientName, doctorName, domainEvent.ScheduledAt);

            try
            {
                var settings = await _settingsRepository.GetSettingsAsync(cancellationToken);
                var ctx = new AppointmentMessageContext(
                    PatientFirstName: patient.PersonName.FirstName,
                    PatientFullName: patientName,
                    DoctorName: $"د. {doctorName}",
                    ClinicName: settings?.ClinicName ?? "عيادتنا",
                    AppointmentDateTime: domainEvent.ScheduledAt,
                    Specialty: doctor.Specialty.ToString());

                var arabicMessage = await _groqService.GenerateAppointmentConfirmationAsync(ctx);
                await _whatsAppService.SendAppointmentConfirmationAsync(patientPhone, arabicMessage);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "WhatsApp confirmation failed for appointment {AppointmentId}. SMS was sent successfully.",
                    domainEvent.AppointmentId);
            }

            var user = await _unitOfWork.Users.GetByPatientIdAsync(domainEvent.PatientId);
            if (user != null)
            {
                var appNotification = MediQueue.Domain.Entities.Notification.Create(
                    user.Id,
                    "Appointment Booked",
                    $"Your appointment with {doctorName} is confirmed for {domainEvent.ScheduledAt:f}",
                    MediQueue.Domain.Entities.NotificationType.Success);
                
                await _unitOfWork.Notifications.AddAsync(appNotification);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }

        var reminderTime = domainEvent.ScheduledAt.AddDays(-1);
        if (reminderTime > DateTime.UtcNow)
        {
            await _schedulerService.ScheduleReminderAsync(domainEvent.AppointmentId, reminderTime);
        }

        await _realtimeService.BroadcastAsync("AppointmentBooked", new { domainEvent.AppointmentId, domainEvent.DoctorId, domainEvent.ScheduledAt });
    }
}

public class AppointmentCancelledEventHandler : INotificationHandler<DomainEventNotification<AppointmentCancelledEvent>>
{
    private readonly ISmsService _smsService;
    private readonly IWhatsAppService _whatsAppService;
    private readonly IGroqService _groqService;
    private readonly ISchedulerService _schedulerService;
    private readonly ICacheService _cacheService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISettingsRepository _settingsRepository;
    private readonly ILogger<AppointmentCancelledEventHandler> _logger;

    public AppointmentCancelledEventHandler(
        ISmsService smsService,
        IWhatsAppService whatsAppService,
        IGroqService groqService,
        ISchedulerService schedulerService,
        ICacheService cacheService,
        IUnitOfWork unitOfWork,
        ISettingsRepository settingsRepository,
        ILogger<AppointmentCancelledEventHandler> logger)
    {
        _smsService = smsService;
        _whatsAppService = whatsAppService;
        _groqService = groqService;
        _schedulerService = schedulerService;
        _cacheService = cacheService;
        _unitOfWork = unitOfWork;
        _settingsRepository = settingsRepository;
        _logger = logger;
    }

    public async Task Handle(DomainEventNotification<AppointmentCancelledEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        var appointment = await _unitOfWork.Appointments.GetByIdAsync(domainEvent.AppointmentId);
        if (appointment != null)
        {
            var patient = await _unitOfWork.Patients.GetByIdAsync(appointment.PatientId);
            var doctor = await _unitOfWork.Doctors.GetByIdAsync(appointment.DoctorId);

            if (patient != null && doctor != null)
            {
                var patientPhone = patient.ContactInfo.Phone
                    ?? patient.ContactInfo.AlternativePhone;

                if (!string.IsNullOrWhiteSpace(patientPhone))
                {
                    await _smsService.SendAppointmentCancellationAsync(patientPhone, domainEvent.Reason);

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

                        var arabicMessage = await _groqService.GenerateAppointmentCancellationAsync(ctx, domainEvent.Reason);
                        await _whatsAppService.SendAppointmentCancellationAsync(patientPhone, arabicMessage);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex,
                            "WhatsApp cancellation failed for appointment {AppointmentId}. SMS was sent successfully.",
                            domainEvent.AppointmentId);
                    }
                }
            }
        }

        await _schedulerService.CancelReminderAsync(domainEvent.AppointmentId.ToString());

        await _cacheService.RemoveByPrefixAsync("availability:");
    }
}

public class AppointmentRescheduledEventHandler : INotificationHandler<DomainEventNotification<AppointmentRescheduledEvent>>
{
    private readonly ISmsService _smsService;
    private readonly IWhatsAppService _whatsAppService;
    private readonly IGroqService _groqService;
    private readonly ISchedulerService _schedulerService;
    private readonly ICacheService _cacheService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISettingsRepository _settingsRepository;
    private readonly ILogger<AppointmentRescheduledEventHandler> _logger;

    public AppointmentRescheduledEventHandler(
        ISmsService smsService,
        IWhatsAppService whatsAppService,
        IGroqService groqService,
        ISchedulerService schedulerService,
        ICacheService cacheService,
        IUnitOfWork unitOfWork,
        ISettingsRepository settingsRepository,
        ILogger<AppointmentRescheduledEventHandler> logger)
    {
        _smsService = smsService;
        _whatsAppService = whatsAppService;
        _groqService = groqService;
        _schedulerService = schedulerService;
        _cacheService = cacheService;
        _unitOfWork = unitOfWork;
        _settingsRepository = settingsRepository;
        _logger = logger;
    }

    public async Task Handle(DomainEventNotification<AppointmentRescheduledEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        var appointment = await _unitOfWork.Appointments.GetByIdAsync(domainEvent.AppointmentId);
        if (appointment != null)
        {
            var patient = await _unitOfWork.Patients.GetByIdAsync(appointment.PatientId);
            var doctor = await _unitOfWork.Doctors.GetByIdAsync(appointment.DoctorId);

            if (patient != null && doctor != null)
            {
                var patientPhone = patient.ContactInfo.Phone
                    ?? patient.ContactInfo.AlternativePhone;

                if (!string.IsNullOrWhiteSpace(patientPhone))
                {
                    await _smsService.SendAppointmentReminderAsync(
                        patientPhone,
                        patient.PersonName.FullName,
                        doctor.PersonName.FullName,
                        domainEvent.NewDateTime);

                    try
                    {
                        var settings = await _settingsRepository.GetSettingsAsync(cancellationToken);
                        var ctx = new AppointmentMessageContext(
                            PatientFirstName: patient.PersonName.FirstName,
                            PatientFullName: patient.PersonName.FullName,
                            DoctorName: $"د. {doctor.PersonName.FullName}",
                            ClinicName: settings?.ClinicName ?? "عيادتنا",
                            AppointmentDateTime: domainEvent.NewDateTime,
                            Specialty: doctor.Specialty.ToString());

                        var arabicMessage = await _groqService.GenerateAppointmentRescheduleAsync(ctx);
                        await _whatsAppService.SendAppointmentConfirmationAsync(patientPhone, arabicMessage);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex,
                            "WhatsApp reschedule failed for appointment {AppointmentId}. SMS was sent successfully.",
                            domainEvent.AppointmentId);
                    }
                }
            }
        }

        await _schedulerService.CancelReminderAsync(domainEvent.AppointmentId.ToString());
        var reminderTime = domainEvent.NewDateTime.AddDays(-1);
        if (reminderTime > DateTime.UtcNow)
        {
            await _schedulerService.ScheduleReminderAsync(domainEvent.AppointmentId, reminderTime);
        }

        await _cacheService.RemoveByPrefixAsync("availability:");
    }
}

public class AppointmentNoShowEventHandler : INotificationHandler<DomainEventNotification<AppointmentNoShowEvent>>
{
    private readonly ISmsService _smsService;
    private readonly IWhatsAppService _whatsAppService;
    private readonly IGroqService _groqService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISettingsRepository _settingsRepository;
    private readonly ILogger<AppointmentNoShowEventHandler> _logger;

    public AppointmentNoShowEventHandler(
        ISmsService smsService,
        IWhatsAppService whatsAppService,
        IGroqService groqService,
        IUnitOfWork unitOfWork,
        ISettingsRepository settingsRepository,
        ILogger<AppointmentNoShowEventHandler> logger)
    {
        _smsService = smsService;
        _whatsAppService = whatsAppService;
        _groqService = groqService;
        _unitOfWork = unitOfWork;
        _settingsRepository = settingsRepository;
        _logger = logger;
    }

    public async Task Handle(DomainEventNotification<AppointmentNoShowEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        var appointment = await _unitOfWork.Appointments.GetByIdAsync(domainEvent.AppointmentId);
        if (appointment != null)
        {
            var patient = await _unitOfWork.Patients.GetByIdAsync(appointment.PatientId);
            var doctor = await _unitOfWork.Doctors.GetByIdAsync(appointment.DoctorId);

            if (patient != null && doctor != null)
            {
                var patientPhone = patient.ContactInfo.Phone
                    ?? patient.ContactInfo.AlternativePhone;

                if (!string.IsNullOrWhiteSpace(patientPhone))
                {
                    await _smsService.SendAppointmentCancellationAsync(
                        patientPhone, "Appointment marked as No-Show.");

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

                        var arabicMessage = await _groqService.GenerateAppointmentCancellationAsync(ctx, "تغيب المريض عن الموعد");
                        await _whatsAppService.SendAppointmentCancellationAsync(patientPhone, arabicMessage);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex,
                            "WhatsApp no-show failed for appointment {AppointmentId}. SMS was sent successfully.",
                            domainEvent.AppointmentId);
                    }
                }
            }
        }
    }
}
