using MediatR;
using MediQueue.Application.Appointments.Commands;

namespace MediQueue.Infrastructure.ExternalServices;

public class MissedAppointmentJob
{
    private readonly ISender _sender;

    public MissedAppointmentJob(ISender sender)
    {
        _sender = sender;
    }

    public async Task ExecuteAsync()
    {
        await _sender.Send(new ProcessMissedAppointmentsCommand());
    }
}
