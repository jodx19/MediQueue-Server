// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Appointments\Commands\StartAppointmentCommand.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Domain.Exceptions;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.Appointments.Commands;

public record StartAppointmentCommand(Guid AppointmentId) : ICommand;

public class StartAppointmentCommandValidator : AbstractValidator<StartAppointmentCommand>
{
    public StartAppointmentCommandValidator()
    {
        RuleFor(x => x.AppointmentId).NotEmpty();
    }
}

public class StartAppointmentCommandHandler : IRequestHandler<StartAppointmentCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public StartAppointmentCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(StartAppointmentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var appointment = await _unitOfWork.Appointments.GetByIdAsync(request.AppointmentId);
            if (appointment == null)
            {
                return Result.Failure($"Appointment with ID '{request.AppointmentId}' was not found.");
            }

            appointment.Start();

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
