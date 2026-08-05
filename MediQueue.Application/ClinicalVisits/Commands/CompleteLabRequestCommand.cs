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

public class CompleteLabRequestCommand : ICommand
{
    public Guid VisitId { get; set; }
    public Guid LabRequestId { get; set; }
    public string ResultValue { get; set; } = string.Empty;
    public string? ResultNotes { get; set; }
    public LabResultStatus Status { get; set; } = LabResultStatus.Completed;
}

public class CompleteLabRequestCommandValidator : AbstractValidator<CompleteLabRequestCommand>
{
    public CompleteLabRequestCommandValidator()
    {
        RuleFor(x => x.VisitId).NotEmpty();
        RuleFor(x => x.LabRequestId).NotEmpty();
        RuleFor(x => x.ResultValue).NotEmpty().WithMessage("Result value is required to mark as complete.");
    }
}

public class CompleteLabRequestCommandHandler : IRequestHandler<CompleteLabRequestCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public CompleteLabRequestCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(CompleteLabRequestCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var visit = await _unitOfWork.ClinicalVisits.GetByIdAsync(request.VisitId);
            if (visit == null)
                return Result.Failure($"ClinicalVisit '{request.VisitId}' was not found.");

            // Find the specific LabRequest inside the visit
            var labRequest = visit.LabRequests.FirstOrDefault(lr => lr.Id == request.LabRequestId);
            if (labRequest == null)
                return Result.Failure($"Lab request '{request.LabRequestId}' was not found in this visit.");

            labRequest.UpdateResult(request.ResultValue, request.ResultNotes, request.Status);

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
