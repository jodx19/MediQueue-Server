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

/// <summary>
/// Updates an existing lab request's test name, instructions, and/or status.
/// Use <see cref="CompleteLabRequestCommand"/> to attach results and mark as complete.
/// </summary>
public class UpdateLabRequestCommand : ICommand<Result>
{
    public Guid VisitId { get; set; }
    public Guid LabRequestId { get; set; }

    /// <summary>Optional — update the test name (e.g. correct a typo).</summary>
    public string? TestName { get; set; }

    /// <summary>Optional — update the instructions for the lab technician.</summary>
    public string? Instructions { get; set; }

    /// <summary>Optional — advance the status (e.g. Pending → InProgress).</summary>
    public LabResultStatus? Status { get; set; }
}

public class UpdateLabRequestCommandValidator : AbstractValidator<UpdateLabRequestCommand>
{
    public UpdateLabRequestCommandValidator()
    {
        RuleFor(x => x.VisitId).NotEmpty();
        RuleFor(x => x.LabRequestId).NotEmpty();

        RuleFor(x => x.TestName)
            .MaximumLength(200)
            .When(x => x.TestName is not null)
            .WithMessage("Test name must not exceed 200 characters.");

        // Do not allow skipping directly from Pending to Completed here;
        // use CompleteLabRequestCommand for that (it requires a result value).
        RuleFor(x => x.Status)
            .Must(s => s != LabResultStatus.Completed && s != LabResultStatus.Abnormal && s != LabResultStatus.Critical)
            .When(x => x.Status.HasValue)
            .WithMessage("Use CompleteLabRequestCommand to mark a lab request as Completed/Abnormal/Critical — it requires a result value.");
    }
}

public class UpdateLabRequestCommandHandler : IRequestHandler<UpdateLabRequestCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateLabRequestCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateLabRequestCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var visit = await _unitOfWork.ClinicalVisits.GetByIdAsync(request.VisitId);
            if (visit == null)
                return Result.Failure($"ClinicalVisit '{request.VisitId}' was not found.");

            var labRequest = visit.LabRequests.FirstOrDefault(lr => lr.Id == request.LabRequestId);
            if (labRequest == null)
                return Result.Failure($"Lab request '{request.LabRequestId}' was not found in this visit.");

            labRequest.UpdateDetails(request.TestName, request.Instructions, request.Status);

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
