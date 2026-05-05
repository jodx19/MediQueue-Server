// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\ClinicalVisits\Commands\AddLabRequestCommand.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Domain.Exceptions;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.ClinicalVisits.Commands;

public class AddLabRequestCommand : ICommand
{
    public Guid VisitId { get; set; }
    public string TestName { get; set; } = string.Empty;
    public string? Instructions { get; set; }
}

public class AddLabRequestCommandValidator : AbstractValidator<AddLabRequestCommand>
{
    public AddLabRequestCommandValidator()
    {
        RuleFor(x => x.VisitId).NotEmpty();
        RuleFor(x => x.TestName).NotEmpty();
    }
}

public class AddLabRequestCommandHandler : IRequestHandler<AddLabRequestCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public AddLabRequestCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(AddLabRequestCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var visit = await _unitOfWork.ClinicalVisits.GetByIdAsync(request.VisitId);
            if (visit == null)
            {
                return Result.Failure($"ClinicalVisit with ID '{request.VisitId}' was not found.");
            }

            visit.AddLabRequest(request.TestName, request.Instructions);

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
