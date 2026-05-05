// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Appointments\Commands\ConfirmAppointmentCommand.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Domain.Exceptions;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.Appointments.Commands;

public record ConfirmAppointmentCommand(Guid AppointmentId) : ICommand;

public class ConfirmAppointmentCommandValidator : AbstractValidator<ConfirmAppointmentCommand>
{
    public ConfirmAppointmentCommandValidator()
    {
        RuleFor(x => x.AppointmentId).NotEmpty();
    }
}

public class ConfirmAppointmentCommandHandler : IRequestHandler<ConfirmAppointmentCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public ConfirmAppointmentCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ConfirmAppointmentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var appointment = await _unitOfWork.Appointments.GetByIdAsync(request.AppointmentId);
            if (appointment == null)
            {
                return Result.Failure($"Appointment with ID '{request.AppointmentId}' was not found.");
            }

            appointment.Confirm();

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
