// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\ClinicalVisits\Commands\AddDiagnosisCommand.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Domain.Enums;
using MediQueue.Domain.Exceptions;
using MediQueue.Domain.Interfaces;
using MediQueue.Domain.ValueObjects;

namespace MediQueue.Application.ClinicalVisits.Commands;

public class AddDiagnosisCommand : ICommand
{
    public Guid VisitId { get; set; }
    public string ICD10Code { get; set; } = string.Empty;
    public string CodeDescription { get; set; } = string.Empty;
    public DiagnosisType DiagnosisType { get; set; }
    public string? Notes { get; set; }
}

public class AddDiagnosisCommandValidator : AbstractValidator<AddDiagnosisCommand>
{
    public AddDiagnosisCommandValidator()
    {
        RuleFor(x => x.VisitId).NotEmpty();
        RuleFor(x => x.ICD10Code)
            .NotEmpty()
            .Matches(@"^[A-Z][0-9]{2}(\.[0-9A-Z]{1,4})?$")
            .WithMessage("Invalid ICD-10 format.");
        RuleFor(x => x.CodeDescription).NotEmpty();
    }
}

public class AddDiagnosisCommandHandler : IRequestHandler<AddDiagnosisCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public AddDiagnosisCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(AddDiagnosisCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var visit = await _unitOfWork.ClinicalVisits.GetByIdAsync(request.VisitId);
            if (visit == null)
            {
                return Result.Failure($"ClinicalVisit with ID '{request.VisitId}' was not found.");
            }

            var medicalCode = new MedicalCode(MedicalCodeSystem.ICD10, request.ICD10Code, request.CodeDescription);
            visit.AddDiagnosis(medicalCode, request.CodeDescription, request.DiagnosisType, request.Notes);

            await _unitOfWork.ClinicalVisits.UpdateAsync(visit);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
