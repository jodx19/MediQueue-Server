// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Appointments\EventHandlers\AppointmentEventHandlers.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MediQueue.Application.Interfaces;
using MediQueue.Domain.Events;
using MediQueue.Domain.Interfaces;

using MediQueue.Application.Common;

namespace MediQueue.Application.Appointments.EventHandlers;

public class AppointmentBookedEventHandler : INotificationHandler<DomainEventNotification<AppointmentBookedEvent>>
{
    private readonly ISmsService _smsService;
    private readonly ISchedulerService _schedulerService;
    private readonly IRealtimeService _realtimeService;
    private readonly IUnitOfWork _unitOfWork;

    public AppointmentBookedEventHandler(
        ISmsService smsService,
        ISchedulerService schedulerService,
        IRealtimeService realtimeService,
        IUnitOfWork unitOfWork)
    {
        _smsService = smsService;
        _schedulerService = schedulerService;
        _realtimeService = realtimeService;
        _unitOfWork = unitOfWork;
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

            // Create In-App Notification
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
    private readonly ISchedulerService _schedulerService;
    private readonly ICacheService _cacheService;

    public AppointmentCancelledEventHandler(
        ISmsService smsService,
        ISchedulerService schedulerService,
        ICacheService cacheService)
    {
        _smsService = smsService;
        _schedulerService = schedulerService;
        _cacheService = cacheService;
    }

    public async Task Handle(DomainEventNotification<AppointmentCancelledEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        var patientPhone = "01000000000"; // Placeholder

        _ = _smsService.SendAppointmentCancellationAsync(patientPhone, domainEvent.Reason);

        _ = _schedulerService.CancelReminderAsync(domainEvent.AppointmentId.ToString());

        _ = _cacheService.RemoveByPrefixAsync("availability:");

        await Task.CompletedTask;
    }
}

public class AppointmentRescheduledEventHandler : INotificationHandler<DomainEventNotification<AppointmentRescheduledEvent>>
{
    private readonly ISmsService _smsService;
    private readonly ISchedulerService _schedulerService;
    private readonly ICacheService _cacheService;

    public AppointmentRescheduledEventHandler(
        ISmsService smsService,
        ISchedulerService schedulerService,
        ICacheService cacheService)
    {
        _smsService = smsService;
        _schedulerService = schedulerService;
        _cacheService = cacheService;
    }

    public async Task Handle(DomainEventNotification<AppointmentRescheduledEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        var patientPhone = "01000000000"; // Placeholder
        var patientName = "Patient"; 
        var doctorName = "Doctor";

        _ = _smsService.SendAppointmentReminderAsync(patientPhone, patientName, doctorName, domainEvent.NewDateTime);

        _ = _schedulerService.CancelReminderAsync(domainEvent.AppointmentId.ToString());
        var reminderTime = domainEvent.NewDateTime.AddDays(-1);
        if (reminderTime > DateTime.UtcNow)
        {
            _ = _schedulerService.ScheduleReminderAsync(domainEvent.AppointmentId, reminderTime);
        }

        _ = _cacheService.RemoveByPrefixAsync("availability:");

        await Task.CompletedTask;
    }
}

public class AppointmentNoShowEventHandler : INotificationHandler<DomainEventNotification<AppointmentNoShowEvent>>
{
    private readonly ISmsService _smsService;

    public AppointmentNoShowEventHandler(ISmsService smsService)
    {
        _smsService = smsService;
    }

    public async Task Handle(DomainEventNotification<AppointmentNoShowEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        var patientPhone = "01000000000"; // Placeholder
        
        _ = _smsService.SendAppointmentCancellationAsync(patientPhone, "Appointment marked as No-Show.");

        await Task.CompletedTask;
    }
}
