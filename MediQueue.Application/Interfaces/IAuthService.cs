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

    /// <summary>
    /// Sends a password-reset email. Always succeeds silently — callers cannot
    /// distinguish whether the email exists (anti-enumeration).
    /// </summary>
    Task ForgotPasswordAsync(string email, CancellationToken ct = default);

    /// <summary>
    /// Validates the reset token and replaces the password. Throws
    /// <see cref="ApplicationException"/> with a generic message on failure.
    /// </summary>
    Task ResetPasswordAsync(
        string            email,
        string            token,
        string            newPassword,
        CancellationToken ct = default);
}
