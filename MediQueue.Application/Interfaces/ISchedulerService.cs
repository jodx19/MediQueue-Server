// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Interfaces\ISchedulerService.cs
using System;
using System.Threading.Tasks;

namespace MediQueue.Application.Interfaces;

/// <summary>
/// Service for scheduling background jobs.
/// </summary>
public interface ISchedulerService
{
    Task<string> ScheduleReminderAsync(Guid appointmentId, DateTime scheduledAt);
    Task CancelReminderAsync(string jobId);
    Task ScheduleRecurringAsync(string jobId, string cronExpression, Action action);
}
