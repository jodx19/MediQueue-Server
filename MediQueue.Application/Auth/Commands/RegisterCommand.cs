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

namespace MediQueue.Application.Auth.Commands;

public class RegisterCommand : IRequest<Result<bool>>
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
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
        RuleFor(x => x.PhoneNumber).NotEmpty();
    }
}

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher<AppUser> _passwordHasher;

    public RegisterCommandHandler(IUnitOfWork unitOfWork, IPasswordHasher<AppUser> passwordHasher)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
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
                    // 2. Create AppUser (Role: Patient by default)
                    var user = AppUser.Create(
                        request.Username,
                        request.Email,
                        request.FirstName,
                        request.LastName,
                        request.PhoneNumber,
                        "", // Hash will be set below
                        UserRole.Patient);

                    var hashedPassword = _passwordHasher.HashPassword(user, request.Password);
                    user.SetPasswordHash(hashedPassword);

                    await _unitOfWork.Users.AddAsync(user);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    // 3. Create Patient Entity
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

                    await _unitOfWork.CommitTransactionAsync();
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
