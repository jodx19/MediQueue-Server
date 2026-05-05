// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Patients\Commands\AddAllergyCommand.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Domain.Entities;
using MediQueue.Domain.Exceptions;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.Patients.Commands;

public class AddAllergyCommand : ICommand
{
    public Guid PatientId { get; set; }
    public string Allergen { get; set; } = string.Empty;
    public AllergySeverity Severity { get; set; }
    public string Reaction { get; set; } = string.Empty;
    public DateOnly? DiagnosedAt { get; set; }
}

public class AddAllergyCommandValidator : AbstractValidator<AddAllergyCommand>
{
    public AddAllergyCommandValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.Allergen).NotEmpty();
        RuleFor(x => x.Reaction).NotEmpty();
    }
}

public class AddAllergyCommandHandler : IRequestHandler<AddAllergyCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public AddAllergyCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(AddAllergyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var patient = await _unitOfWork.Patients.GetByIdAsync(request.PatientId);
            if (patient == null)
            {
                return Result.Failure($"Patient with ID '{request.PatientId}' was not found.");
            }

            patient.AddAllergy(request.Allergen, request.Severity, request.Reaction);

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
