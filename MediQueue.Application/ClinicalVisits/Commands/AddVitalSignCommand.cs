// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\ClinicalVisits\Commands\AddVitalSignCommand.cs
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

public class AddVitalSignCommand : ICommand
{
    public Guid VisitId { get; set; }
    public VitalSignType VitalSignType { get; set; }
    public decimal Value { get; set; }
    public string Unit { get; set; } = string.Empty;
}

public class AddVitalSignCommandValidator : AbstractValidator<AddVitalSignCommand>
{
    public AddVitalSignCommandValidator()
    {
        RuleFor(x => x.VisitId).NotEmpty();
        RuleFor(x => x.Unit).NotEmpty();
        // More validation based on VitalSignType could be added
    }
}

public class AddVitalSignCommandHandler : IRequestHandler<AddVitalSignCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public AddVitalSignCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(AddVitalSignCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var visit = await _unitOfWork.ClinicalVisits.GetByIdAsync(request.VisitId);
            if (visit == null)
            {
                return Result.Failure($"ClinicalVisit with ID '{request.VisitId}' was not found.");
            }

            var vitalSign = new VitalSign(request.VitalSignType, request.Value, request.Unit, DateTime.UtcNow);
            visit.AddVitalSign(vitalSign);

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
