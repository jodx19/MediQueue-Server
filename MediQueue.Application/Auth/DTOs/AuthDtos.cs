// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Auth\DTOs\AuthDtos.cs
using System;
using MediQueue.Domain.Entities;

namespace MediQueue.Application.Auth.DTOs;

public record LoginRequestDto(string Email, string Password);

public record RegisterRequestDto(
    string Username, 
    string Email, 
    string Password,
    string FirstName,
    string LastName,
    string PhoneNumber);

public record RefreshTokenRequestDto(string Token, string RefreshToken);

public record AuthResponseDto(
    string Token, 
    string RefreshToken, 
    DateTime ExpiryTime, 
    string Username, 
    string Role);
