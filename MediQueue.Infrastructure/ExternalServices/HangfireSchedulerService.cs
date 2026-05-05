using System;
using System.Threading.Tasks;
using Hangfire;
using MediQueue.Application.Interfaces;

namespace MediQueue.Infrastructure.ExternalServices;

public class HangfireSchedulerService : ISchedulerService
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IRecurringJobManager _recurringJobManager;

    public HangfireSchedulerService(IBackgroundJobClient backgroundJobClient, IRecurringJobManager recurringJobManager)
    {
        _backgroundJobClient = backgroundJobClient;
        _recurringJobManager = recurringJobManager;
    }

    public async Task<string> ScheduleReminderAsync(Guid appointmentId, DateTime scheduledAt)
    {
        // Example: schedule reminder 2 hours before the appointment
        var enqueueAt = scheduledAt.AddHours(-2);
        
        // This is a placeholder for actual reminder logic which should be implemented in an application handler or notification service
        var jobId = _backgroundJobClient.Schedule<ISmsService>(
            service => service.SendAppointmentReminderAsync("phone", "patient name", "doctor name", scheduledAt),
            new DateTimeOffset(enqueueAt));
            
        return await Task.FromResult(jobId);
    }

    public async Task CancelReminderAsync(string jobId)
    {
        _backgroundJobClient.Delete(jobId);
        await Task.CompletedTask;
    }

    public async Task ScheduleRecurringAsync(string jobId, string cronExpression, Action action)
    {
        _recurringJobManager.AddOrUpdate(jobId, () => action(), cronExpression);
        await Task.CompletedTask;
    }
}
