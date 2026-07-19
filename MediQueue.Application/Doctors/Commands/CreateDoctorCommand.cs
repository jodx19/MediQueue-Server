// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Doctors\Commands\CreateDoctorCommand.cs
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Application.Interfaces;
using MediQueue.Application.Doctors.DTOs;
using MediQueue.Domain.Entities;
using MediQueue.Domain.Enums;
using MediQueue.Domain.Exceptions;
using MediQueue.Domain.Interfaces;
using MediQueue.Domain.ValueObjects;

namespace MediQueue.Application.Doctors.Commands;

public class CreateDoctorCommand : ICommand<DoctorDto>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public MedicalSpecialty Specialty { get; set; }
    public string? SubSpecialty { get; set; }
    public string LicenseNumber { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public decimal ConsultationFee { get; set; }
    public decimal FollowUpFee { get; set; }
    public string? Bio { get; set; }
    public int YearsOfExperience { get; set; }
    public List<QualificationDto> Qualifications { get; set; } = [];
}

public class CreateDoctorCommandValidator : AbstractValidator<CreateDoctorCommand>
{
    public CreateDoctorCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().Length(2, 50);
        RuleFor(x => x.LastName).NotEmpty().Length(2, 50);
        RuleFor(x => x.LicenseNumber).NotEmpty();
        RuleFor(x => x.Phone).NotEmpty();
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
        RuleFor(x => x.ConsultationFee).GreaterThan(0);
        RuleFor(x => x.FollowUpFee).GreaterThan(0);
        RuleFor(x => x.YearsOfExperience).InclusiveBetween(0, 60);
    }
}

public class CreateDoctorCommandHandler : IRequestHandler<CreateDoctorCommand, Result<DoctorDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IUsageValidatorService _usageValidatorService;
    private readonly ITenantContext _tenantContext;

    public CreateDoctorCommandHandler(
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

    public async Task<Result<DoctorDto>> Handle(CreateDoctorCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var tenantId = _tenantContext.TenantId;
            if (tenantId != Guid.Empty)
            {
                var isQuotaAvailable = await _usageValidatorService.IsQuotaAvailableAsync(tenantId, QuotaType.Doctors);
                if (!isQuotaAvailable)
                {
                    return Result<DoctorDto>.Failure("لقد تخطيت الحد الأقصى للأطباء المسموح به في باقتك الحالية.");
                }
            }

            var personName = new PersonName(request.FirstName, request.LastName);
            var contactInfo = new ContactInfo(request.Phone, request.Email);
            var consultationFee = new Money(request.ConsultationFee);
            var followUpFee = new Money(request.FollowUpFee);

            var doctor = Doctor.Create(
                personName,
                request.Specialty,
                request.LicenseNumber,
                contactInfo,
                consultationFee,
                followUpFee,
                request.SubSpecialty,
                request.Bio,
                request.YearsOfExperience);

            await _unitOfWork.Doctors.AddAsync(doctor);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = _mapper.Map<DoctorDto>(doctor);
            return Result<DoctorDto>.Success(dto);
        }
        catch (DomainException ex)
        {
            return Result<DoctorDto>.Failure(ex.Message);
        }
    }
}
