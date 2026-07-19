// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Appointments\Commands\CancelAppointmentCommand.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Application.Interfaces;
using MediQueue.Domain.Exceptions;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.Appointments.Commands;

public class CancelAppointmentCommand : ICommand
{
    public Guid   AppointmentId { get; set; }
    public string Reason        { get; set; } = string.Empty;
}

public class CancelAppointmentCommandValidator : AbstractValidator<CancelAppointmentCommand>
{
    public CancelAppointmentCommandValidator()
    {
        RuleFor(x => x.AppointmentId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MinimumLength(10)
            .WithMessage("Cancellation reason must be at least 10 characters long.");
    }
}

public class CancelAppointmentCommandHandler : IRequestHandler<CancelAppointmentCommand, Result>
{
    private readonly IUnitOfWork         _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public CancelAppointmentCommandHandler(
        IUnitOfWork         unitOfWork,
        ICurrentUserService currentUser)
    {
        _unitOfWork  = unitOfWork;
        _currentUser = currentUser;
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

            // ── Ownership enforcement ──────────────────────────────────────────
            // Staff (Admin, Receptionist, Doctor) may cancel any appointment.
            // A Patient may only cancel their OWN appointment.
            if (_currentUser.IsInRole("Patient"))
            {
                // The Patient's own PatientId is stored in the JWT claim.
                var callerPatientId = _currentUser.PatientId;

                if (callerPatientId is null || callerPatientId.Value != appointment.PatientId)
                {
                    // Return a generic Forbidden result — do not disclose whose
                    // appointment it is.
                    return Result.Failure(
                        "You do not have permission to cancel this appointment.");
                }
            }
            // ────────────────────────────────────────────────────────────────────

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
