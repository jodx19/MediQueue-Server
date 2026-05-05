// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Appointments\EventHandlers\AppointmentEventHandlers.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MediQueue.Application.Interfaces;
using MediQueue.Domain.Events;

namespace MediQueue.Application.Appointments.EventHandlers;

public class AppointmentBookedEventHandler : INotificationHandler<AppointmentBookedEvent>
{
    private readonly ISmsService _smsService;
    private readonly ISchedulerService _schedulerService;
    private readonly IRealtimeService _realtimeService;

    public AppointmentBookedEventHandler(
        ISmsService smsService,
        ISchedulerService schedulerService,
        IRealtimeService realtimeService)
    {
        _smsService = smsService;
        _schedulerService = schedulerService;
        _realtimeService = realtimeService;
    }

    public async Task Handle(AppointmentBookedEvent notification, CancellationToken cancellationToken)
    {
        var patientName = "Patient"; 
        var patientPhone = "01000000000";
        var doctorName = "Doctor";

        _ = _smsService.SendAppointmentConfirmationAsync(patientPhone, patientName, doctorName, notification.ScheduledAt);

        var reminderTime = notification.ScheduledAt.AddDays(-1);
        if (reminderTime > DateTime.UtcNow)
        {
            _ = _schedulerService.ScheduleReminderAsync(notification.AppointmentId, reminderTime);
        }

        _ = _realtimeService.BroadcastAsync("AppointmentBooked", new { notification.AppointmentId, notification.DoctorId, notification.ScheduledAt });

        await Task.CompletedTask;
    }
}

public class AppointmentCancelledEventHandler : INotificationHandler<AppointmentCancelledEvent>
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

    public async Task Handle(AppointmentCancelledEvent notification, CancellationToken cancellationToken)
    {
        var patientPhone = "01000000000"; // Placeholder

        _ = _smsService.SendAppointmentCancellationAsync(patientPhone, notification.Reason);

        _ = _schedulerService.CancelReminderAsync(notification.AppointmentId.ToString());

        _ = _cacheService.RemoveByPrefixAsync("availability:");

        await Task.CompletedTask;
    }
}

public class AppointmentRescheduledEventHandler : INotificationHandler<AppointmentRescheduledEvent>
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

    public async Task Handle(AppointmentRescheduledEvent notification, CancellationToken cancellationToken)
    {
        var patientPhone = "01000000000"; // Placeholder
        var patientName = "Patient"; 
        var doctorName = "Doctor";

        _ = _smsService.SendAppointmentReminderAsync(patientPhone, patientName, doctorName, notification.NewDateTime);

        _ = _schedulerService.CancelReminderAsync(notification.AppointmentId.ToString());
        var reminderTime = notification.NewDateTime.AddDays(-1);
        if (reminderTime > DateTime.UtcNow)
        {
            _ = _schedulerService.ScheduleReminderAsync(notification.AppointmentId, reminderTime);
        }

        _ = _cacheService.RemoveByPrefixAsync("availability:");

        await Task.CompletedTask;
    }
}

public class AppointmentNoShowEventHandler : INotificationHandler<AppointmentNoShowEvent>
{
    private readonly ISmsService _smsService;

    public AppointmentNoShowEventHandler(ISmsService smsService)
    {
        _smsService = smsService;
    }

    public async Task Handle(AppointmentNoShowEvent notification, CancellationToken cancellationToken)
    {
        var patientPhone = "01000000000"; // Placeholder
        
        _ = _smsService.SendAppointmentCancellationAsync(patientPhone, "Appointment marked as No-Show.");

        await Task.CompletedTask;
    }
}
