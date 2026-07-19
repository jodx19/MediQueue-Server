using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Domain.Exceptions;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.Patients.Commands;

/// <summary>
/// Command to remove a chronic condition from a patient's medical record.
/// </summary>
public record RemoveChronicConditionCommand(Guid PatientId, Guid ConditionId) : ICommand;

public class RemoveChronicConditionCommandHandler : IRequestHandler<RemoveChronicConditionCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public RemoveChronicConditionCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RemoveChronicConditionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var patient = await _unitOfWork.Patients.GetByIdAsync(request.PatientId);
            if (patient == null)
            {
                return Result.Failure($"Patient with ID '{request.PatientId}' was not found.");
            }

            patient.RemoveChronicCondition(request.ConditionId);

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
