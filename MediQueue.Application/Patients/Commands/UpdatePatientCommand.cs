// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Patients\Commands\UpdatePatientCommand.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Application.Patients.DTOs;
using MediQueue.Domain.Exceptions;
using MediQueue.Domain.Interfaces;
using MediQueue.Domain.ValueObjects;

namespace MediQueue.Application.Patients.Commands;

public class UpdatePatientCommand : ICommand<PatientDto>
{
    public Guid PatientId { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? AlternativePhone { get; set; }
}

public class UpdatePatientCommandValidator : AbstractValidator<UpdatePatientCommand>
{
    public UpdatePatientCommandValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.Phone).NotEmpty().Matches(@"^01[0125][0-9]{8}$");
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
    }
}

public class UpdatePatientCommandHandler : IRequestHandler<UpdatePatientCommand, Result<PatientDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdatePatientCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PatientDto>> Handle(UpdatePatientCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var patient = await _unitOfWork.Patients.GetByIdAsync(request.PatientId);
            if (patient == null)
            {
                return Result<PatientDto>.Failure($"Patient with ID '{request.PatientId}' was not found.");
            }

            var contactInfo = new ContactInfo(request.Phone, request.Email, request.AlternativePhone);
            patient.UpdateContactInfo(contactInfo);

            await _unitOfWork.Patients.UpdateAsync(patient);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = _mapper.Map<PatientDto>(patient);
            return Result<PatientDto>.Success(dto);
        }
        catch (DomainException ex)
        {
            return Result<PatientDto>.Failure(ex.Message);
        }
    }
}
