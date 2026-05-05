// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\ClinicalVisits\Commands\CreateClinicalVisitCommand.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Application.ClinicalVisits.DTOs;
using MediQueue.Domain.Entities;
using MediQueue.Domain.Enums;
using MediQueue.Domain.Exceptions;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.ClinicalVisits.Commands;

public class CreateClinicalVisitCommand : ICommand<ClinicalVisitDto>
{
    public Guid AppointmentId { get; set; }
}

public class CreateClinicalVisitCommandValidator : AbstractValidator<CreateClinicalVisitCommand>
{
    public CreateClinicalVisitCommandValidator()
    {
        RuleFor(x => x.AppointmentId).NotEmpty();
    }
}

public class CreateClinicalVisitCommandHandler : IRequestHandler<CreateClinicalVisitCommand, Result<ClinicalVisitDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateClinicalVisitCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ClinicalVisitDto>> Handle(CreateClinicalVisitCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var appointment = await _unitOfWork.Appointments.GetByIdAsync(request.AppointmentId);
            if (appointment == null)
            {
                return Result<ClinicalVisitDto>.Failure($"Appointment with ID '{request.AppointmentId}' was not found.");
            }

            if (appointment.Status != AppointmentStatus.InProgress)
            {
                return Result<ClinicalVisitDto>.Failure("Clinical visit can only be created for appointments that are InProgress.");
            }

            var existingVisit = await _unitOfWork.ClinicalVisits.GetByAppointmentIdAsync(request.AppointmentId);
            if (existingVisit != null)
            {
                return Result<ClinicalVisitDto>.Failure($"A clinical visit already exists for appointment '{request.AppointmentId}'.");
            }

            var visit = ClinicalVisit.Create(appointment.Id, appointment.DoctorId, appointment.PatientId, DateTime.UtcNow);

            await _unitOfWork.ClinicalVisits.AddAsync(visit);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = _mapper.Map<ClinicalVisitDto>(visit);
            return Result<ClinicalVisitDto>.Success(dto);
        }
        catch (DomainException ex)
        {
            return Result<ClinicalVisitDto>.Failure(ex.Message);
        }
    }
}
