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

namespace MediQueue.Application.Patients.Commands.SelfRegister;

public record SelfRegisterPatientCommand(
    string FirstName,
    string LastName,
    string? MiddleName,
    DateTime DateOfBirth,
    string Gender,
    string NationalId,
    string Phone,
    string? Email,
    string? BloodType,
    string? Address,
    string? City,
    string? Governorate
) : ICommand<PatientDto>;

public class SelfRegisterPatientCommandValidator : AbstractValidator<SelfRegisterPatientCommand>
{
    public SelfRegisterPatientCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().Length(2, 50);
        RuleFor(x => x.LastName).NotEmpty().Length(2, 50);
        RuleFor(x => x.NationalId).NotEmpty().Length(14).Matches(@"^\d{14}$");
        RuleFor(x => x.Phone).NotEmpty().Matches(@"^01[0125][0-9]{8}$");
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
        RuleFor(x => x.DateOfBirth).NotEmpty().LessThan(DateTime.UtcNow);
        RuleFor(x => x.Gender).NotEmpty().Must(g =>
            g.Equals("Male", StringComparison.OrdinalIgnoreCase) ||
            g.Equals("Female", StringComparison.OrdinalIgnoreCase));
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
                return Result<PatientDto>.Failure($"A patient with National ID '{request.NationalId}' already exists.");

            var gender = request.Gender.Equals("Male", StringComparison.OrdinalIgnoreCase)
                ? Gender.Male : Gender.Female;

            var bloodType = Enum.TryParse<BloodType>(request.BloodType, true, out var bt)
                ? bt : BloodType.Unknown;

            var personName = new PersonName(request.FirstName, request.LastName, request.MiddleName);
            var contactInfo = new ContactInfo(request.Phone, request.Email);
            var address = new Address(
                request.Address ?? "Not provided",
                request.City ?? "Not provided",
                request.Governorate ?? "Not provided");

            var patient = Patient.Register(
                personName,
                DateOnly.FromDateTime(request.DateOfBirth),
                gender,
                bloodType,
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
