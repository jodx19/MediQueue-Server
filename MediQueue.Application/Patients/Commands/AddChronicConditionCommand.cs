// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Patients\Commands\AddChronicConditionCommand.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Domain.Exceptions;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.Patients.Commands;

public class AddChronicConditionCommand : ICommand
{
    public Guid PatientId { get; set; }
    public string ConditionName { get; set; } = string.Empty;
    public string? ICD10Code { get; set; }
    public DateOnly? DiagnosedAt { get; set; }
    public string? Notes { get; set; }
}

public class AddChronicConditionCommandValidator : AbstractValidator<AddChronicConditionCommand>
{
    public AddChronicConditionCommandValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.ConditionName).NotEmpty();
    }
}

public class AddChronicConditionCommandHandler : IRequestHandler<AddChronicConditionCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public AddChronicConditionCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(AddChronicConditionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var patient = await _unitOfWork.Patients.GetByIdAsync(request.PatientId);
            if (patient == null)
            {
                return Result.Failure($"Patient with ID '{request.PatientId}' was not found.");
            }

            patient.AddChronicCondition(request.ConditionName, request.DiagnosedAt, request.Notes);
            // In a real app we might also set the ICD10Code, but Patient entity logic might need to accept it.
            // Based on domain model, ChronicCondition has ICD10Code but AddChronicCondition doesn't explicitly ask for it in Domain method AddChronicCondition(string name, DateOnly? diagnosedAt = null, string? notes = null) 
            // We follow the Domain method signature.
            // Wait, the domain has `AddChronicCondition(string name, DateOnly? diagnosedAt = null, string? notes = null)`. 
            // So we just use that.

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
