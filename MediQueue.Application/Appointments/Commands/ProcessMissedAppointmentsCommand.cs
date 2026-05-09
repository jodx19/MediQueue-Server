// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Appointments\Commands\ProcessMissedAppointmentsCommand.cs
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.Appointments.Commands;

/// <summary>
/// Background command to scan and update missed appointments to No-Show status.
/// </summary>
public record ProcessMissedAppointmentsCommand : IRequest<Result<int>>;

public class ProcessMissedAppointmentsCommandHandler : IRequestHandler<ProcessMissedAppointmentsCommand, Result<int>>
{
    private readonly IUnitOfWork _unitOfWork;

    public ProcessMissedAppointmentsCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int>> Handle(ProcessMissedAppointmentsCommand request, CancellationToken cancellationToken)
    {
        // Appointments scheduled more than 30 minutes ago and still in Scheduled/Confirmed status
        var threshold = DateTime.UtcNow.AddMinutes(-30);
        var missedAppointments = await _unitOfWork.Appointments.GetMissedAppointmentsAsync(threshold);

        if (!missedAppointments.Any())
        {
            return Result<int>.Success(0);
        }

        int updatedCount = 0;
        foreach (var appointment in missedAppointments)
        {
            try
            {
                appointment.MarkNoShow();
                await _unitOfWork.Appointments.UpdateAsync(appointment);
                updatedCount++;
            }
            catch (Exception)
            {
                // Continue processing others
                continue;
            }
        }

        if (updatedCount > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result<int>.Success(updatedCount);
    }
}
