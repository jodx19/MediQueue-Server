using System;
using System.Threading.Tasks;
using MediQueue.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace MediQueue.Infrastructure.Services;

/// <summary>
/// A simple scheduler service for development that just logs actions instead of using Hangfire.
/// </summary>
public class DevelopmentSchedulerService : ISchedulerService
{
    private readonly ILogger<DevelopmentSchedulerService> _logger;

    public DevelopmentSchedulerService(ILogger<DevelopmentSchedulerService> logger)
    {
        _logger = logger;
    }

    public Task<string> ScheduleReminderAsync(Guid appointmentId, DateTime scheduledAt)
    {
        var jobId = Guid.NewGuid().ToString();
        _logger.LogInformation("[DevScheduler] Scheduled reminder for Appointment {AppointmentId} at {ScheduledAt}. JobId: {JobId}", 
            appointmentId, scheduledAt, jobId);
        return Task.FromResult(jobId);
    }

    public Task CancelReminderAsync(string jobId)
    {
        _logger.LogInformation("[DevScheduler] Canceled job {JobId}", jobId);
        return Task.CompletedTask;
    }

    public Task ScheduleRecurringAsync(string jobId, string cronExpression, Action action)
    {
        _logger.LogInformation("[DevScheduler] Scheduled recurring job {JobId} with cron {Cron}", jobId, cronExpression);
        // We don't execute the action here to keep it simple
        return Task.CompletedTask;
    }
}
