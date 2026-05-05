// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Auth\DTOs\AuthDtos.cs
using System;
using MediQueue.Domain.Entities;

namespace MediQueue.Application.Auth.DTOs;

public record LoginRequestDto(string Username, string Password);

public record RegisterRequestDto(
    string Username, 
    string Email, 
    string Password, 
    UserRole Role, 
    Guid? DoctorId = null, 
    Guid? PatientId = null);

public record RefreshTokenRequestDto(string Token, string RefreshToken);

public record AuthResponseDto(
    string Token, 
    string RefreshToken, 
    DateTime ExpiryTime, 
    string Username, 
    string Role);
