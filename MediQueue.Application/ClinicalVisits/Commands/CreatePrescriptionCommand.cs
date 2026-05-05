// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\ClinicalVisits\Commands\CreatePrescriptionCommand.cs
using System;
using System.Collections.Generic;
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
using MediQueue.Domain.ValueObjects;

namespace MediQueue.Application.ClinicalVisits.Commands;

public class CreatePrescriptionCommand : ICommand<PrescriptionDto>
{
    public Guid VisitId { get; set; }
    public List<PrescriptionItemDto> Items { get; set; } = [];
    public DateTime ValidUntil { get; set; }
}

public class CreatePrescriptionCommandValidator : AbstractValidator<CreatePrescriptionCommand>
{
    public CreatePrescriptionCommandValidator()
    {
        RuleFor(x => x.VisitId).NotEmpty();
        RuleFor(x => x.ValidUntil).GreaterThan(DateTime.UtcNow);
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(items =>
        {
            items.RuleFor(i => i.MedicationName).NotEmpty();
            items.RuleFor(i => i.Dosage).NotEmpty();
            items.RuleFor(i => i.Frequency).NotEmpty();
            items.RuleFor(i => i.Duration).NotEmpty();
        });
    }
}

public class CreatePrescriptionCommandHandler : IRequestHandler<CreatePrescriptionCommand, Result<PrescriptionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreatePrescriptionCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PrescriptionDto>> Handle(CreatePrescriptionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var visit = await _unitOfWork.ClinicalVisits.GetByIdAsync(request.VisitId);
            if (visit == null)
            {
                return Result<PrescriptionDto>.Failure($"ClinicalVisit with ID '{request.VisitId}' was not found.");
            }

            var domainItems = request.Items.Select(i => 
                new PrescriptionItem(
                    i.MedicationName, 
                    i.Dosage, 
                    i.Form, 
                    i.Frequency, 
                    i.Duration, 
                    i.Quantity, 
                    i.GenericName,
                    i.Instructions,
                    i.Refills)
            ).ToList();

            visit.CreatePrescription(domainItems, request.ValidUntil);

            await _unitOfWork.ClinicalVisits.UpdateAsync(visit);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = _mapper.Map<PrescriptionDto>(visit.Prescriptions.LastOrDefault());
            return Result<PrescriptionDto>.Success(dto);
        }
        catch (DomainException ex)
        {
            return Result<PrescriptionDto>.Failure(ex.Message);
        }
    }
}
