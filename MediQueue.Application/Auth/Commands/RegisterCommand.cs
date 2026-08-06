// Path: MediQueue.Application/Auth/Commands/RegisterCommand.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using MediQueue.Application.Auth.DTOs;
using MediQueue.Application.Common;
using MediQueue.Application.Interfaces;
using MediQueue.Domain.Entities;
using MediQueue.Domain.Enums;
using MediQueue.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;
using MediQueue.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace MediQueue.Application.Auth.Commands;

public class RegisterCommand : IRequest<Result<bool>>
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Role { get; set; } // "Admin", "Doctor", "Receptionist", "Patient"
}

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MinimumLength(3);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
        RuleFor(x => x.FirstName).NotEmpty();
        RuleFor(x => x.LastName).NotEmpty();
    }
}

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher<AppUser> _passwordHasher;
    private readonly IEmailService _emailService;
    private readonly IAppSettingsService _appSettings;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        IUnitOfWork unitOfWork,
        IPasswordHasher<AppUser> passwordHasher,
        IEmailService emailService,
        IAppSettingsService appSettings,
        ILogger<RegisterCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _emailService = emailService;
        _appSettings = appSettings;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // 1. Check if user already exists
        var existingUser = await _unitOfWork.Users.GetByUsernameAsync(request.Username);
        if (existingUser != null) return Result<bool>.Failure("Username already exists.");

        var existingEmail = await _unitOfWork.Users.GetByEmailAsync(request.Email);
        if (existingEmail != null) return Result<bool>.Failure("Email already exists.");

        try
        {
            await _unitOfWork.ExecuteStrategyAsync(async () =>
            {
                await _unitOfWork.BeginTransactionAsync();
                try
                {
                    // Parse role
                    UserRole userRole = UserRole.Patient;
                    if (!string.IsNullOrEmpty(request.Role) && Enum.TryParse<UserRole>(request.Role, true, out var parsedRole))
                    {
                        userRole = parsedRole;
                    }

                    // 2. Create AppUser
                    var user = AppUser.Create(
                        request.Username,
                        request.Email,
                        request.FirstName,
                        request.LastName,
                        request.PhoneNumber,
                        "", // Hash will be set below
                        userRole);

                    var hashedPassword = _passwordHasher.HashPassword(user, request.Password);
                    user.SetPasswordHash(hashedPassword);

                    await _unitOfWork.Users.AddAsync(user);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    // 3. Conditional entity creation based on role
                    if (userRole == UserRole.Patient)
                    {
                        var personName = new PersonName(request.FirstName, request.LastName);
                        var contactInfo = new ContactInfo(request.PhoneNumber, request.Email);
                        
                        var patient = Patient.Register(
                            personName,
                            DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-20)), // Default DOB placeholder
                            Gender.Other,
                            BloodType.Unknown,
                            "TEMP-" + Guid.NewGuid().ToString()[..8],
                            contactInfo,
                            new Address("Default", "Default", "Default"),
                            MaritalStatus.Single);

                        await _unitOfWork.Patients.AddAsync(patient);
                        await _unitOfWork.SaveChangesAsync(cancellationToken);

                        // 4. Link User to Patient
                        user.LinkToPatient(patient.Id);
                        await _unitOfWork.Users.UpdateAsync(user);
                        await _unitOfWork.SaveChangesAsync(cancellationToken);
                    }
                    else if (userRole == UserRole.Doctor)
                    {
                        var doc = Doctor.Create(
                            new PersonName(request.FirstName, request.LastName),
                            MedicalSpecialty.GeneralPractice,
                            "LIC-" + Guid.NewGuid().ToString()[..8],
                            new ContactInfo(request.PhoneNumber, request.Email),
                            new Money(100),
                            new Money(50));
                            
                        await _unitOfWork.Doctors.AddAsync(doc);
                        await _unitOfWork.SaveChangesAsync(cancellationToken);

                        user.GetType().GetProperty("DoctorId")?.SetValue(user, doc.Id);
                        await _unitOfWork.Users.UpdateAsync(user);
                        await _unitOfWork.SaveChangesAsync(cancellationToken);
                    }

                    await _unitOfWork.CommitTransactionAsync();

                    // Send email verification (outside transaction — email failure must not roll back registration)
                    var verificationToken = Guid.NewGuid().ToString("N");
                    user.GenerateEmailVerificationToken(verificationToken);
                    await _unitOfWork.Users.UpdateAsync(user);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    var frontendUrl = _appSettings.FrontendUrl;
                    var verificationLink = $"{frontendUrl}/verify-email?userId={user.Id}&token={Uri.EscapeDataString(verificationToken)}";

                    try
                    {
                        await _emailService.SendVerificationEmailAsync(user.Email, verificationLink);
                    }
                    catch (Exception emailEx)
                    {
                        _logger.LogWarning(emailEx, "Failed to send verification email to {Email}. Registration succeeded.", user.Email);
                        // Non-fatal — user can request resend later
                    }
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw; // Re-throw to let ExecutionStrategy handle it or catch it outside
                }
            });

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Registration failed: {ex.Message}");
        }
    }
}
