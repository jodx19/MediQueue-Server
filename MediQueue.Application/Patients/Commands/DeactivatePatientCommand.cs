// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Patients\Commands\DeactivatePatientCommand.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Domain.Exceptions;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.Patients.Commands;

public record DeactivatePatientCommand(Guid PatientId, string Reason = "") : ICommand;

public class DeactivatePatientCommandValidator : AbstractValidator<DeactivatePatientCommand>
{
    public DeactivatePatientCommandValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty();
    }
}

public class DeactivatePatientCommandHandler : IRequestHandler<DeactivatePatientCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeactivatePatientCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeactivatePatientCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var patient = await _unitOfWork.Patients.GetByIdAsync(request.PatientId);
            if (patient == null)
            {
                return Result.Failure($"Patient with ID '{request.PatientId}' was not found.");
            }

            patient.Deactivate();

            await _unitOfWork.Patients.UpdateAsync(patient);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
