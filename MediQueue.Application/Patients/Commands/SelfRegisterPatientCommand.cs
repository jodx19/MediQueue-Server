using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Application.Patients.DTOs;
using MediQueue.Domain.Entities;
using MediQueue.Domain.Enums;
using MediQueue.Domain.Exceptions;
using MediQueue.Domain.Interfaces;
using MediQueue.Domain.ValueObjects;

namespace MediQueue.Application.Patients.Commands;

public class SelfRegisterPatientCommand : ICommand<PatientDto>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public BloodType BloodType { get; set; }
    public string NationalId { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Address { get; set; }
}

public class SelfRegisterPatientCommandValidator : AbstractValidator<SelfRegisterPatientCommand>
{
    public SelfRegisterPatientCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().Length(2, 50).Matches(@"^[a-zA-Z\s]+$");
        RuleFor(x => x.LastName).NotEmpty().Length(2, 50).Matches(@"^[a-zA-Z\s]+$");
        RuleFor(x => x.NationalId).NotEmpty().Length(14).Matches(@"^\d{14}$");
        RuleFor(x => x.Phone).NotEmpty().Matches(@"^01[0125][0-9]{8}$");
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
        RuleFor(x => x.DateOfBirth).NotEmpty().LessThan(DateOnly.FromDateTime(DateTime.UtcNow))
            .Must(dob => dob.Year > DateTime.UtcNow.Year - 120).WithMessage("Age must be less than 120 years.");
    }
}

public class SelfRegisterPatientCommandHandler : IRequestHandler<SelfRegisterPatientCommand, Result<PatientDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SelfRegisterPatientCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PatientDto>> Handle(SelfRegisterPatientCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var existingPatient = await _unitOfWork.Patients.GetByNationalIdAsync(request.NationalId);
            if (existingPatient != null)
            {
                return Result<PatientDto>.Failure($"A patient with National ID '{request.NationalId}' already exists.");
            }

            var personName = new PersonName(request.FirstName, request.LastName);
            var contactInfo = new ContactInfo(request.Phone, request.Email);
            
            // Address mapping: parse or default
            string street = request.Address ?? "Self Registered";
            string city = "Cairo";
            string governorate = "Cairo";
            var address = new Address(street, city, governorate);

            var patient = Patient.Register(
                personName,
                request.DateOfBirth,
                request.Gender,
                request.BloodType,
                request.NationalId,
                contactInfo,
                address,
                MaritalStatus.Single);

            await _unitOfWork.Patients.AddAsync(patient);
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
