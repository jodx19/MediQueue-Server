// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Appointments\Commands\RescheduleAppointmentCommand.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Domain.Exceptions;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.Appointments.Commands;

public class RescheduleAppointmentCommand : ICommand
{
    public Guid AppointmentId { get; set; }
    public DateTime NewScheduledAt { get; set; }
}

public class RescheduleAppointmentCommandValidator : AbstractValidator<RescheduleAppointmentCommand>
{
    public RescheduleAppointmentCommandValidator()
    {
        RuleFor(x => x.AppointmentId).NotEmpty();
        RuleFor(x => x.NewScheduledAt).GreaterThan(DateTime.UtcNow).WithMessage("New scheduled time must be in the future.");
    }
}

public class RescheduleAppointmentCommandHandler : IRequestHandler<RescheduleAppointmentCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public RescheduleAppointmentCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RescheduleAppointmentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var appointment = await _unitOfWork.Appointments.GetByIdAsync(request.AppointmentId);
            if (appointment == null)
            {
                return Result.Failure($"Appointment with ID '{request.AppointmentId}' was not found.");
            }

            // Check if doctor has a conflict
            var hasConflict = await _unitOfWork.Appointments.HasConflictAsync(appointment.DoctorId, request.NewScheduledAt, appointment.DurationMinutes);
            if (hasConflict)
            {
                throw new AppointmentConflictException(appointment.DoctorId, request.NewScheduledAt);
            }

            appointment.Reschedule(request.NewScheduledAt);

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
