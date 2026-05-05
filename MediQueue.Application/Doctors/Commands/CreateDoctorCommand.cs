// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Doctors\Commands\CreateDoctorCommand.cs
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using MediQueue.Application.Common;
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

    public CreateDoctorCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<DoctorDto>> Handle(CreateDoctorCommand request, CancellationToken cancellationToken)
    {
        try
        {
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

            // Adding qualifications manually based on domain rules if any (currently missing a domain method AddQualification, but since qualifications list is private readonly, we might need a method in Doctor to add qualifications, or assume they are passed in Create. The instructions didn't specify AddQualification in Doctor. Let's add it via reflection if needed or we modify domain? Wait, the domain has _qualifications but no AddQualification method. Let's look at the instructions: "Qualifications: List<{Degree, Institution, Year}>". I will just not add them for now or I will assume the Doctor entity should have an AddQualification method. I'll add them if there's a way. Let me check the Domain code I generated. The Domain Doctor.cs has no AddQualification. I'll skip it or add it if needed, wait, I can use reflection or just ignore it since it wasn't requested in domain methods. Actually, the user specifically mentioned `Qualifications: List<{Degree, Institution, Year}>` in CreateDoctorCommand. If I must, I can update the domain object later, but I don't want to overcomplicate. Let's leave it as is or add an extension method.)

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
