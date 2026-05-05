// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Appointments\Commands\CheckInAppointmentCommand.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Domain.Exceptions;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.Appointments.Commands;

public record CheckInAppointmentCommand(Guid AppointmentId) : ICommand;

public class CheckInAppointmentCommandValidator : AbstractValidator<CheckInAppointmentCommand>
{
    public CheckInAppointmentCommandValidator()
    {
        RuleFor(x => x.AppointmentId).NotEmpty();
    }
}

public class CheckInAppointmentCommandHandler : IRequestHandler<CheckInAppointmentCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public CheckInAppointmentCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(CheckInAppointmentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var appointment = await _unitOfWork.Appointments.GetByIdAsync(request.AppointmentId);
            if (appointment == null)
            {
                return Result.Failure($"Appointment with ID '{request.AppointmentId}' was not found.");
            }

            appointment.CheckIn();

            await _unitOfWork.Appointments.UpdateAsync(appointment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
