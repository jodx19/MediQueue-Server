// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Appointments\Commands\CancelAppointmentCommand.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Domain.Exceptions;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.Appointments.Commands;

public class CancelAppointmentCommand : ICommand
{
    public Guid AppointmentId { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class CancelAppointmentCommandValidator : AbstractValidator<CancelAppointmentCommand>
{
    public CancelAppointmentCommandValidator()
    {
        RuleFor(x => x.AppointmentId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MinimumLength(10).WithMessage("Cancellation reason must be at least 10 characters long.");
    }
}

public class CancelAppointmentCommandHandler : IRequestHandler<CancelAppointmentCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public CancelAppointmentCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(CancelAppointmentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var appointment = await _unitOfWork.Appointments.GetByIdAsync(request.AppointmentId);
            if (appointment == null)
            {
                return Result.Failure($"Appointment with ID '{request.AppointmentId}' was not found.");
            }

            appointment.Cancel(request.Reason);

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
