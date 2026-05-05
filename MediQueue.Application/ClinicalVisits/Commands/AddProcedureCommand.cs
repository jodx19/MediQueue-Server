// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\ClinicalVisits\Commands\AddProcedureCommand.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Domain.Exceptions;
using MediQueue.Domain.Interfaces;
using MediQueue.Domain.ValueObjects;

namespace MediQueue.Application.ClinicalVisits.Commands;

public class AddProcedureCommand : ICommand
{
    public Guid VisitId { get; set; }
    public string CPTCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Fee { get; set; }
}

public class AddProcedureCommandValidator : AbstractValidator<AddProcedureCommand>
{
    public AddProcedureCommandValidator()
    {
        RuleFor(x => x.VisitId).NotEmpty();
        RuleFor(x => x.CPTCode).NotEmpty();
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.Fee).GreaterThanOrEqualTo(0);
    }
}

public class AddProcedureCommandHandler : IRequestHandler<AddProcedureCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public AddProcedureCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(AddProcedureCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var visit = await _unitOfWork.ClinicalVisits.GetByIdAsync(request.VisitId);
            if (visit == null)
            {
                return Result.Failure($"ClinicalVisit with ID '{request.VisitId}' was not found.");
            }

            var medicalCode = new MedicalCode(MedicalCodeSystem.CPT, request.CPTCode, request.Description);
            visit.AddProcedure(medicalCode, request.Description, new Money(request.Fee));

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
