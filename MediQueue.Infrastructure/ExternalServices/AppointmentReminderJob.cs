using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MediQueue.Application.Appointments.Commands;

namespace MediQueue.Infrastructure.ExternalServices;

public class AppointmentReminderJob
{
    private readonly ISender _sender;

    public AppointmentReminderJob(ISender sender)
        => _sender = sender;

    public async Task ExecuteAsync(Guid appointmentId)
    {
        await _sender.Send(
            new SendAppointmentReminderCommand(appointmentId),
            CancellationToken.None);
    }
}
