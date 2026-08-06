using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Domain.Enums;
using MediQueue.Domain.Exceptions;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.ClinicalVisits.Commands;

public class UpdateLabRequestCommand : ICommand
{
    public Guid VisitId { get; set; }
    public Guid LabRequestId { get; set; }
    public LabResultStatus Status { get; set; }
    public string ResultValue { get; set; } = string.Empty;
    public string? ResultNotes { get; set; }
}

public class UpdateLabRequestCommandValidator : AbstractValidator<UpdateLabRequestCommand>
{
    public UpdateLabRequestCommandValidator()
    {
        RuleFor(x => x.VisitId).NotEmpty();
        RuleFor(x => x.LabRequestId).NotEmpty();
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.ResultValue).NotEmpty();
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
            {
                return Result.Failure($"ClinicalVisit with ID '{request.VisitId}' was not found.");
            }

            var labRequest = visit.LabRequests.FirstOrDefault(lr => lr.Id == request.LabRequestId);
            if (labRequest == null)
            {
                return Result.Failure($"LabRequest with ID '{request.LabRequestId}' was not found in visit.");
            }

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
