// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Interfaces\IAuthService.cs
using System.Threading.Tasks;
using MediQueue.Application.Common;
using MediQueue.Application.Auth.DTOs;

namespace MediQueue.Application.Interfaces;

public interface IAuthService
{
    Task<Result<AuthResponseDto>> LoginAsync(LoginRequestDto request);
    Task<Result<AuthResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto request);
    Task<Result<AuthResponseDto>> PatientLoginAsync(string mrn, DateTime dateOfBirth);
}
