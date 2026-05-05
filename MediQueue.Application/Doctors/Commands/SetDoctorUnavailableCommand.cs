// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Doctors\Commands\SetDoctorUnavailableCommand.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Domain.Exceptions;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.Doctors.Commands;

public class SetDoctorUnavailableCommand : ICommand
{
    public Guid DoctorId { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class SetDoctorUnavailableCommandValidator : AbstractValidator<SetDoctorUnavailableCommand>
{
    public SetDoctorUnavailableCommandValidator()
    {
        RuleFor(x => x.DoctorId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty();
    }
}

public class SetDoctorUnavailableCommandHandler : IRequestHandler<SetDoctorUnavailableCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public SetDoctorUnavailableCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(SetDoctorUnavailableCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var doctor = await _unitOfWork.Doctors.GetByIdAsync(request.DoctorId);
            if (doctor == null)
            {
                return Result.Failure($"Doctor with ID '{request.DoctorId}' was not found.");
            }

            doctor.SetUnavailable(request.Reason);

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
