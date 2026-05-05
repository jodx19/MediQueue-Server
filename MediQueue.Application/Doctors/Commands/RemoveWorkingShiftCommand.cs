// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Doctors\Commands\RemoveWorkingShiftCommand.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Domain.Exceptions;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.Doctors.Commands;

public record RemoveWorkingShiftCommand(Guid DoctorId, DayOfWeek DayOfWeek) : ICommand;

public class RemoveWorkingShiftCommandHandler : IRequestHandler<RemoveWorkingShiftCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public RemoveWorkingShiftCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RemoveWorkingShiftCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var doctor = await _unitOfWork.Doctors.GetByIdAsync(request.DoctorId);
            if (doctor == null)
            {
                return Result.Failure($"Doctor with ID '{request.DoctorId}' was not found.");
            }

            doctor.RemoveWorkingShift(request.DayOfWeek);

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
