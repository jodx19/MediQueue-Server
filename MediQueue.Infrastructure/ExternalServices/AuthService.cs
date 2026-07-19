// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Infrastructure\ExternalServices\AuthService.cs
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MediQueue.Application.Auth.DTOs;
using MediQueue.Application.Common;
using MediQueue.Application.Interfaces;
using MediQueue.Domain.Entities;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Infrastructure.ExternalServices;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly IPasswordHasher<AppUser> _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;

    public AuthService(
        IUnitOfWork unitOfWork, 
        IConfiguration configuration,
        IPasswordHasher<AppUser> passwordHasher,
        ITokenService tokenService,
        IEmailService emailService)
    {
        _unitOfWork    = unitOfWork;
        _configuration = configuration;
        _passwordHasher = passwordHasher;
        _tokenService  = tokenService;
        _emailService  = emailService;
    }

    public async Task<Result<AuthResponseDto>> LoginAsync(LoginRequestDto request)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);
        if (user == null || !user.IsActive)
        {
            return Result<AuthResponseDto>.Failure("Invalid email or password.");
        }

        var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verificationResult == PasswordVerificationResult.Failed)
        {
            return Result<AuthResponseDto>.Failure("Invalid email or password.");
        }

        var tokenString = _tokenService.GenerateJwtToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();
        
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "60");
        var expiryTime = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var tokenResponse = new AuthResponseDto(tokenString, refreshToken, expiryTime, user.Username, user.Role.ToString());
        
        user.UpdateRefreshToken(tokenResponse.RefreshToken, DateTime.UtcNow.AddDays(7));
        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return Result<AuthResponseDto>.Success(tokenResponse);
    }

    public async Task<Result<AuthResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return Result<AuthResponseDto>.Failure("Refresh token is required.");
        }

        var user = await _unitOfWork.Users.GetByRefreshTokenAsync(request.RefreshToken);
        if (user == null || !user.IsActive)
        {
            return Result<AuthResponseDto>.Failure("Invalid refresh token.");
        }

        if (!user.RefreshTokenExpiryTime.HasValue || user.RefreshTokenExpiryTime.Value <= DateTime.UtcNow)
        {
            return Result<AuthResponseDto>.Failure("Refresh token has expired.");
        }

        var accessToken = _tokenService.GenerateJwtToken(user);
        var newRefreshToken = _tokenService.GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddMinutes(
            int.Parse(_configuration.GetSection("JwtSettings")["ExpiryMinutes"] ?? "60"));

        user.UpdateRefreshToken(newRefreshToken, DateTime.UtcNow.AddDays(7));
        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var response = new AuthResponseDto(
            accessToken,
            newRefreshToken,
            expiresAt,
            user.Username,
            user.Role.ToString());

        return Result<AuthResponseDto>.Success(response);
    }

    public async Task<Result<AuthResponseDto>> PatientLoginAsync(string mrn, DateTime dateOfBirth)
    {
        var patient = await _unitOfWork.Patients.GetByMRNAsync(mrn);
        if (patient == null || !patient.IsActive)
            return Result<AuthResponseDto>.Failure("Invalid MRN or Date of Birth.");

        var patientDob = patient.DateOfBirth;
        var requestDob = DateOnly.FromDateTime(dateOfBirth);
        if (patientDob != requestDob)
            return Result<AuthResponseDto>.Failure("Invalid MRN or Date of Birth.");

        var user = await _unitOfWork.Users.GetByPatientIdAsync(patient.Id);
        if (user == null || !user.IsActive)
            return Result<AuthResponseDto>.Failure("No account found for this patient.");

        var tokenString = _tokenService.GenerateJwtToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        var jwtSettings = _configuration.GetSection("JwtSettings");
        var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "60");
        var expiryTime = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var tokenResponse = new AuthResponseDto(tokenString, refreshToken, expiryTime, user.Username, user.Role.ToString());

        user.UpdateRefreshToken(tokenResponse.RefreshToken, DateTime.UtcNow.AddDays(7));
        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return Result<AuthResponseDto>.Success(tokenResponse);
    }

    /// <inheritdoc />
    public async Task LogoutAsync(string userId, CancellationToken ct = default)
    {
        // Silent: do NOT reveal whether the user exists. The /logout endpoint
        // returns 204 unconditionally — probing a userId by calling logout
        // must not be a discovery oracle.
        if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var id))
            return;

        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user is null)
            return;

        user.RevokeRefreshToken();
        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task ForgotPasswordAsync(
        string            email,
        CancellationToken ct = default)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(email);

        // Silent return — never reveal whether the email exists.
        if (user is null) return;

        // Generate and persist the reset token (valid 15 min).
        var token = user.RequestPasswordReset();
        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync(ct);

        // Build the reset link (frontend URL — configure via appsettings in production).
        var resetLink =
            $"https://mediqueue.app/auth/reset-password" +
            $"?email={Uri.EscapeDataString(email)}" +
            $"&token={token}";

        var subject = "MediQueue — Password Reset Request";
        var body    = $"""
            Dear {user.FirstName ?? "User"},

            You requested to reset your MediQueue password.

            Click the link below to reset your password
            (valid for 15 minutes):

            {resetLink}

            If you did not request this, please ignore this email.
            Your password will not be changed.

            — MediQueue Security Team
            """;

        await _emailService.SendEmailAsync(email, subject, body);
    }

    /// <inheritdoc />
    public async Task ResetPasswordAsync(
        string            email,
        string            token,
        string            newPassword,
        CancellationToken ct = default)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(email);

        // Unified error — never expose the failure reason.
        const string errorMsg = "Invalid or expired password reset token.";

        if (user is null)
            throw new ApplicationException(errorMsg);

        if (!user.IsPasswordResetTokenValid(token))
            throw new ApplicationException(errorMsg);

        // Hash and apply the new password (same mechanism as Register flow).
        var hashedPassword = _passwordHasher.HashPassword(user, newPassword);
        user.SetPasswordHash(hashedPassword);

        // Invalidate the token (single-use enforcement).
        user.ClearPasswordResetToken();

        // Force logout from all devices by revoking the refresh token.
        user.RevokeRefreshToken();

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
