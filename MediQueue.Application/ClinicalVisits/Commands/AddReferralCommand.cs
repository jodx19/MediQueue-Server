// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\ClinicalVisits\Commands\AddReferralCommand.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Domain.Enums;
using MediQueue.Domain.Exceptions;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.ClinicalVisits.Commands;

public class AddReferralCommand : ICommand
{
    public Guid VisitId { get; set; }
    public MedicalSpecialty ReferredToSpecialty { get; set; }
    public string Reason { get; set; } = string.Empty;
    public ReferralUrgency Urgency { get; set; }
    public string? Notes { get; set; }
}

public class AddReferralCommandValidator : AbstractValidator<AddReferralCommand>
{
    public AddReferralCommandValidator()
    {
        RuleFor(x => x.VisitId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty();
    }
}

public class AddReferralCommandHandler : IRequestHandler<AddReferralCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public AddReferralCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(AddReferralCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var visit = await _unitOfWork.ClinicalVisits.GetByIdAsync(request.VisitId);
            if (visit == null)
            {
                return Result.Failure($"ClinicalVisit with ID '{request.VisitId}' was not found.");
            }

            visit.AddReferral(request.ReferredToSpecialty, request.Reason, request.Urgency, notes: request.Notes);

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
