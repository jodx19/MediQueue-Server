using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using MediQueue.Application.Auth.DTOs;
using MediQueue.Application.Common;
using MediQueue.Application.Interfaces;
using MediQueue.Domain.Entities;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.Auth.Commands;

public class PatientLoginCommand : IRequest<Result<AuthResponseDto>>
{
    public string MRN { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
}

public class PatientLoginCommandValidator : AbstractValidator<PatientLoginCommand>
{
    public PatientLoginCommandValidator()
    {
        RuleFor(x => x.MRN).NotEmpty().WithMessage("Medical Record Number (MRN) is required.");
        RuleFor(x => x.DateOfBirth).NotEmpty().WithMessage("Date of Birth is required.");
    }
}

public class PatientLoginCommandHandler : IRequestHandler<PatientLoginCommand, Result<AuthResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;

    public PatientLoginCommandHandler(IUnitOfWork unitOfWork, ITokenService tokenService)
    {
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
    }

    public async Task<Result<AuthResponseDto>> Handle(PatientLoginCommand request, CancellationToken cancellationToken)
    {
        var patient = await _unitOfWork.Patients.GetByMRNAsync(request.MRN.Trim());
        if (patient == null)
        {
            return Result<AuthResponseDto>.Failure("Invalid MRN or Date of Birth.");
        }

        if (patient.DateOfBirth != request.DateOfBirth)
        {
            return Result<AuthResponseDto>.Failure("Invalid MRN or Date of Birth.");
        }

        if (!patient.IsActive)
        {
            return Result<AuthResponseDto>.Failure("This patient account has been deactivated.");
        }

        // Retrieve existing AppUser for this patient, or create a persistent one if missing
        var user = await _unitOfWork.Users.GetByPatientIdAsync(patient.Id);
        if (user == null)
        {
            var username = patient.MedicalRecordNumber.Replace("-", "").ToLower();
            var email = patient.ContactInfo.Email ?? $"{username}@mediqueue.local";
            
            user = AppUser.Create(
                username,
                email,
                patient.PersonName.FirstName,
                patient.PersonName.LastName,
                patient.ContactInfo.Phone,
                passwordHash: string.Empty, // Patients log in via MRN+DOB
                role: UserRole.Patient,
                doctorId: null,
                patientId: patient.Id
            );

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        if (!user.IsActive)
        {
            return Result<AuthResponseDto>.Failure("User account is deactivated.");
        }

        var tokenString = _tokenService.GenerateJwtToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();
        
        // Expiry time (default to 60 minutes)
        var expiryTime = DateTime.UtcNow.AddMinutes(60);

        user.UpdateRefreshToken(refreshToken, DateTime.UtcNow.AddDays(7));
        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new AuthResponseDto(tokenString, refreshToken, expiryTime, user.Username, "Patient");
        return Result<AuthResponseDto>.Success(response);
    }
}
