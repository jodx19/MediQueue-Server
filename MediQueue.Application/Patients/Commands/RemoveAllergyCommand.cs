// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Patients\Commands\RemoveAllergyCommand.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Domain.Exceptions;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.Patients.Commands;

public record RemoveAllergyCommand(Guid PatientId, Guid AllergyId) : ICommand;

public class RemoveAllergyCommandHandler : IRequestHandler<RemoveAllergyCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public RemoveAllergyCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RemoveAllergyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var patient = await _unitOfWork.Patients.GetByIdAsync(request.PatientId);
            if (patient == null)
            {
                return Result.Failure($"Patient with ID '{request.PatientId}' was not found.");
            }

            patient.RemoveAllergy(request.AllergyId);

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
