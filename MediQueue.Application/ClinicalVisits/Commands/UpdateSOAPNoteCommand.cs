// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\ClinicalVisits\Commands\UpdateSOAPNoteCommand.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Domain.Exceptions;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.ClinicalVisits.Commands;

public class UpdateSOAPNoteCommand : ICommand
{
    public Guid VisitId { get; set; }
    public string? SubjectiveNote { get; set; }
    public string? ObjectiveNote { get; set; }
    public string? AssessmentNote { get; set; }
    public string? PlanNote { get; set; }
}

public class UpdateSOAPNoteCommandValidator : AbstractValidator<UpdateSOAPNoteCommand>
{
    public UpdateSOAPNoteCommandValidator()
    {
        RuleFor(x => x.VisitId).NotEmpty();
    }
}

public class UpdateSOAPNoteCommandHandler : IRequestHandler<UpdateSOAPNoteCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateSOAPNoteCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateSOAPNoteCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var visit = await _unitOfWork.ClinicalVisits.GetByIdAsync(request.VisitId);
            if (visit == null)
            {
                return Result.Failure($"ClinicalVisit with ID '{request.VisitId}' was not found.");
            }

            visit.UpdateSOAPNotes(request.SubjectiveNote, request.ObjectiveNote, request.AssessmentNote, request.PlanNote);

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
