// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Appointments\Commands\MarkNoShowCommand.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.Appointments.Commands;

/// <summary>
/// Command to mark an appointment as a no-show.
/// </summary>
public record MarkNoShowCommand(Guid AppointmentId) : ICommand<Result>;

/// <summary>
/// Handles the <see cref="MarkNoShowCommand"/>.
/// </summary>
public class MarkNoShowCommandHandler : IRequestHandler<MarkNoShowCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public MarkNoShowCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(MarkNoShowCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _unitOfWork.Appointments.GetByIdAsync(request.AppointmentId);

        if (appointment is null)
            return Result.Failure($"Appointment '{request.AppointmentId}' not found.");

        try
        {
            appointment.MarkNoShow();
            await _unitOfWork.Appointments.UpdateAsync(appointment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Domain.Exceptions.DomainException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
