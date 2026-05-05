// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Appointments\Commands\CompleteAppointmentCommand.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Domain.Exceptions;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.Appointments.Commands;

public record CompleteAppointmentCommand(Guid AppointmentId) : ICommand;

public class CompleteAppointmentCommandValidator : AbstractValidator<CompleteAppointmentCommand>
{
    public CompleteAppointmentCommandValidator()
    {
        RuleFor(x => x.AppointmentId).NotEmpty();
    }
}

public class CompleteAppointmentCommandHandler : IRequestHandler<CompleteAppointmentCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public CompleteAppointmentCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(CompleteAppointmentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var appointment = await _unitOfWork.Appointments.GetByIdAsync(request.AppointmentId);
            if (appointment == null)
            {
                return Result.Failure($"Appointment with ID '{request.AppointmentId}' was not found.");
            }

            appointment.Complete();

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
