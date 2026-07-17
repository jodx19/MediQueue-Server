// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Interfaces\IAuthService.cs
using System.Threading;
using System.Threading.Tasks;
using MediQueue.Application.Common;
using MediQueue.Application.Auth.DTOs;

namespace MediQueue.Application.Interfaces;

public interface IAuthService
{
    Task<Result<AuthResponseDto>> LoginAsync(LoginRequestDto request);
    Task<Result<AuthResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto request);
    Task<Result<AuthResponseDto>> PatientLoginAsync(string mrn, DateTime dateOfBirth);

    /// <summary>
    /// Revokes the current refresh token for the user identified by
    /// <paramref name="userId"/>. Used by the logout flow to prevent the
    /// previously-issued refresh token from minting new access tokens.
    /// Silently no-ops if the user is not found — callers should not be able
    /// to distinguish "user exists" from "user does not exist".
    /// </summary>
    Task LogoutAsync(string userId, CancellationToken ct = default);
}
