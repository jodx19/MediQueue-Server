// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\ClinicalVisits\Commands\FinalizeClinicalVisitCommand.cs
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Application.ClinicalVisits.DTOs;
using MediQueue.Domain.Exceptions;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.ClinicalVisits.Commands;

public record FinalizeClinicalVisitCommand(Guid VisitId) : ICommand<ClinicalVisitSummaryDto>;

public class FinalizeClinicalVisitCommandValidator : AbstractValidator<FinalizeClinicalVisitCommand>
{
    public FinalizeClinicalVisitCommandValidator()
    {
        RuleFor(x => x.VisitId).NotEmpty();
    }
}

public class FinalizeClinicalVisitCommandHandler : IRequestHandler<FinalizeClinicalVisitCommand, Result<ClinicalVisitSummaryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public FinalizeClinicalVisitCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ClinicalVisitSummaryDto>> Handle(FinalizeClinicalVisitCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var visit = await _unitOfWork.ClinicalVisits.GetByIdAsync(request.VisitId);
            if (visit == null)
            {
                return Result<ClinicalVisitSummaryDto>.Failure($"ClinicalVisit with ID '{request.VisitId}' was not found.");
            }

            // Validations as requested in rules:
            if (string.IsNullOrWhiteSpace(visit.SubjectiveNote) ||
                string.IsNullOrWhiteSpace(visit.ObjectiveNote) ||
                string.IsNullOrWhiteSpace(visit.AssessmentNote) ||
                string.IsNullOrWhiteSpace(visit.PlanNote))
            {
                return Result<ClinicalVisitSummaryDto>.Failure("All 4 SOAP notes must be non-empty to finalize.");
            }

            if (!visit.Diagnoses.Any())
            {
                return Result<ClinicalVisitSummaryDto>.Failure("At least 1 diagnosis must be added to finalize.");
            }

            visit.FinalizeVisit();

            await _unitOfWork.ClinicalVisits.UpdateAsync(visit);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = _mapper.Map<ClinicalVisitSummaryDto>(visit);
            return Result<ClinicalVisitSummaryDto>.Success(dto);
        }
        catch (DomainException ex)
        {
            return Result<ClinicalVisitSummaryDto>.Failure(ex.Message);
        }
    }
}
