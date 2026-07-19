// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Patients\Commands\RegisterPatientCommand.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Application.Interfaces;
using MediQueue.Application.Patients.DTOs;
using MediQueue.Domain.Entities;
using MediQueue.Domain.Enums;
using MediQueue.Domain.Exceptions;
using MediQueue.Domain.Interfaces;
using MediQueue.Domain.ValueObjects;

namespace MediQueue.Application.Patients.Commands;

public class RegisterPatientCommand : ICommand<PatientDto>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public BloodType BloodType { get; set; }
    public string NationalId { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Governorate { get; set; } = string.Empty;
    public MaritalStatus MaritalStatus { get; set; }
    public string? InsuranceProvider { get; set; }
    public string? InsurancePolicyNumber { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
}

public class RegisterPatientCommandValidator : AbstractValidator<RegisterPatientCommand>
{
    public RegisterPatientCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().Length(2, 50).Matches(@"^[a-zA-Z\s]+$");
        RuleFor(x => x.LastName).NotEmpty().Length(2, 50).Matches(@"^[a-zA-Z\s]+$");
        RuleFor(x => x.NationalId).NotEmpty().Length(14).Matches(@"^\d{14}$");
        RuleFor(x => x.Phone).NotEmpty().Matches(@"^01[0125][0-9]{8}$");
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
        RuleFor(x => x.DateOfBirth).NotEmpty().LessThan(DateOnly.FromDateTime(DateTime.UtcNow))
            .Must(dob => dob.Year > DateTime.UtcNow.Year - 120).WithMessage("Age must be less than 120 years.");
        RuleFor(x => x.Street).NotEmpty();
        RuleFor(x => x.City).NotEmpty();
        RuleFor(x => x.Governorate).NotEmpty();
    }
}

public class RegisterPatientCommandHandler : IRequestHandler<RegisterPatientCommand, Result<PatientDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IUsageValidatorService _usageValidatorService;
    private readonly ITenantContext _tenantContext;

    public RegisterPatientCommandHandler(
        IUnitOfWork unitOfWork, 
        IMapper mapper, 
        IUsageValidatorService usageValidatorService,
        ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _usageValidatorService = usageValidatorService;
        _tenantContext = tenantContext;
    }

    public async Task<Result<PatientDto>> Handle(RegisterPatientCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var tenantId = _tenantContext.TenantId;
            if (tenantId != Guid.Empty)
            {
                var isQuotaAvailable = await _usageValidatorService.IsQuotaAvailableAsync(tenantId, QuotaType.Patients);
                if (!isQuotaAvailable)
                {
                    return Result<PatientDto>.Failure("لقد تخطيت الحد الأقصى للمرضى المسموح به في باقتك الحالية.");
                }
            }

            var existingPatient = await _unitOfWork.Patients.GetByNationalIdAsync(request.NationalId);
            if (existingPatient != null)
            {
                return Result<PatientDto>.Failure($"A patient with National ID '{request.NationalId}' already exists.");
            }

            var personName = new PersonName(request.FirstName, request.LastName, request.MiddleName);
            var contactInfo = new ContactInfo(request.Phone, request.Email);
            var address = new Address(request.Street, request.City, request.Governorate);

            var patient = Patient.Register(
                personName,
                request.DateOfBirth,
                request.Gender,
                request.BloodType,
                request.NationalId,
                contactInfo,
                address,
                request.MaritalStatus,
                request.EmergencyContactName,
                request.EmergencyContactPhone,
                request.InsuranceProvider,
                request.InsurancePolicyNumber);

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

