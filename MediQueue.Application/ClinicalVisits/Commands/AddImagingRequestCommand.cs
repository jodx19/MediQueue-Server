// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\ClinicalVisits\Commands\AddImagingRequestCommand.cs
using System;
using MediQueue.Domain.Enums;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Domain.Exceptions;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.ClinicalVisits.Commands;

public class AddImagingRequestCommand : ICommand
{
    public Guid VisitId { get; set; }
    public string ImagingType { get; set; } = string.Empty;
    public string BodyPart { get; set; } = string.Empty;
    public string? Instructions { get; set; }
}

public class AddImagingRequestCommandValidator : AbstractValidator<AddImagingRequestCommand>
{
    public AddImagingRequestCommandValidator()
    {
        RuleFor(x => x.VisitId).NotEmpty();
        RuleFor(x => x.ImagingType).NotEmpty();
        RuleFor(x => x.BodyPart).NotEmpty();
    }
}

public class AddImagingRequestCommandHandler : IRequestHandler<AddImagingRequestCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public AddImagingRequestCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(AddImagingRequestCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var visit = await _unitOfWork.ClinicalVisits.GetByIdAsync(request.VisitId);
            if (visit == null)
            {
                return Result.Failure($"ClinicalVisit with ID '{request.VisitId}' was not found.");
            }

            if (!Enum.TryParse<ImagingType>(request.ImagingType, true, out var imagingType))
            {
                return Result.Failure($"Invalid imaging type: {request.ImagingType}");
            }

            visit.AddImagingRequest(imagingType, request.BodyPart, request.Instructions);

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
