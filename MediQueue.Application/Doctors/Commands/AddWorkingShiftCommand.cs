// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Doctors\Commands\AddWorkingShiftCommand.cs
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Domain.Exceptions;
using MediQueue.Domain.Interfaces;
using MediQueue.Domain.ValueObjects;

namespace MediQueue.Application.Doctors.Commands;

public class AddWorkingShiftCommand : ICommand
{
    public Guid DoctorId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int SlotDurationMinutes { get; set; }
}

public class AddWorkingShiftCommandValidator : AbstractValidator<AddWorkingShiftCommand>
{
    public AddWorkingShiftCommandValidator()
    {
        RuleFor(x => x.DoctorId).NotEmpty();
        RuleFor(x => x.StartTime).LessThan(x => x.EndTime).WithMessage("Start time must be before end time.");
        RuleFor(x => x.SlotDurationMinutes).GreaterThan(0);
    }
}

public class AddWorkingShiftCommandHandler : IRequestHandler<AddWorkingShiftCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public AddWorkingShiftCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(AddWorkingShiftCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var doctor = await _unitOfWork.Doctors.GetByIdAsync(request.DoctorId);
            if (doctor == null)
            {
                return Result.Failure($"Doctor with ID '{request.DoctorId}' was not found.");
            }

            // Check for overlapping shifts
            if (doctor.WorkingShifts.Any(s => s.DayOfWeek == request.DayOfWeek))
            {
                return Result.Failure($"Doctor already has a shift on {request.DayOfWeek}.");
            }

            var shift = new WorkingShift(
                request.DayOfWeek,
                request.StartTime,
                request.EndTime,
                request.SlotDurationMinutes);

            doctor.AddWorkingShift(shift);

            await _unitOfWork.Doctors.UpdateAsync(doctor);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
