// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Infrastructure\ExternalServices\AuthService.cs
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
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

    public AuthService(
        IUnitOfWork unitOfWork, 
        IConfiguration configuration,
        IPasswordHasher<AppUser> passwordHasher)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<AuthResponseDto>> LoginAsync(LoginRequestDto request)
    {
        var user = await _unitOfWork.Users.GetByUsernameAsync(request.Username);
        if (user == null || !user.IsActive)
        {
            return Result<AuthResponseDto>.Failure("Invalid username or password.");
        }

        var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verificationResult == PasswordVerificationResult.Failed)
        {
            return Result<AuthResponseDto>.Failure("Invalid username or password.");
        }

        var tokenResponse = GenerateJwtToken(user);
        
        user.UpdateRefreshToken(tokenResponse.RefreshToken, DateTime.UtcNow.AddDays(7));
        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return Result<AuthResponseDto>.Success(tokenResponse);
    }

    public async Task<Result<bool>> RegisterAsync(RegisterRequestDto request)
    {
        var existingUser = await _unitOfWork.Users.GetByUsernameAsync(request.Username);
        if (existingUser != null)
        {
            return Result<bool>.Failure("Username already exists.");
        }

        var existingEmail = await _unitOfWork.Users.GetByEmailAsync(request.Email);
        if (existingEmail != null)
        {
            return Result<bool>.Failure("Email already exists.");
        }

        var user = AppUser.Create(
            request.Username, 
            request.Email, 
            "", 
            request.Role, 
            request.DoctorId, 
            request.PatientId);

        var hashedPassword = _passwordHasher.HashPassword(user, request.Password);
        user.SetPasswordHash(hashedPassword);

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return Result<bool>.Success(true);
    }

    public async Task<Result<AuthResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto request)
    {
        // Implementation for RefreshToken
        return await Task.FromResult(Result<AuthResponseDto>.Failure("Not implemented yet"));
    }

    private AuthResponseDto GenerateJwtToken(AppUser user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("SecretKey is missing.");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("Email", user.Email)
        };

        if (user.DoctorId.HasValue) claims.Add(new Claim("DoctorId", user.DoctorId.Value.ToString()));
        if (user.PatientId.HasValue) claims.Add(new Claim("PatientId", user.PatientId.Value.ToString()));

        var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "60");
        var expiryTime = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: expiryTime,
            signingCredentials: creds
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        return new AuthResponseDto(tokenString, refreshToken, expiryTime, user.Username, user.Role.ToString());
    }
}
