// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Domain\Entities\AppUser.cs
using System;
using MediQueue.Domain.Common;

namespace MediQueue.Domain.Entities;

public enum UserRole
{
    Admin        = 1,
    Doctor       = 2,
    Patient      = 3,
    Receptionist = 4,
    SuperAdmin   = 5
}

public class AppUser : AuditableEntity
{
    public string Username { get; private set; }
    public string Email { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string PasswordHash { get; private set; }
    public UserRole Role { get; private set; }
    public Guid? DoctorId { get; private set; }
    public Guid? PatientId { get; private set; }
    public bool IsActive { get; private set; }
    public string? RefreshToken { get; private set; }
    public DateTime? RefreshTokenExpiryTime { get; private set; }

    // Password Reset
    public string?   PasswordResetToken          { get; private set; }
    public DateTime? PasswordResetTokenExpiresAt { get; private set; }

    private AppUser() 
    { 
        Username = null!;
        Email = null!;
        FirstName = null!;
        LastName = null!;
        PasswordHash = null!;
    }

    private AppUser(string username, string email, string firstName, string lastName, string? phoneNumber, string passwordHash, UserRole role, Guid? doctorId = null, Guid? patientId = null)
    {
        Username = username;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
        PasswordHash = passwordHash;
        Role = role;
        DoctorId = doctorId;
        PatientId = patientId;
        IsActive = true;
    }

    public static AppUser Create(string username, string email, string firstName, string lastName, string? phoneNumber, string passwordHash, UserRole role, Guid? doctorId = null, Guid? patientId = null)
    {
        return new AppUser(username, email, firstName, lastName, phoneNumber, passwordHash, role, doctorId, patientId);
    }

    public void SetPasswordHash(string passwordHash)
    {
        PasswordHash = passwordHash;
    }

    public void UpdateRefreshToken(string refreshToken, DateTime expiryTime)
    {
        RefreshToken = refreshToken;
        RefreshTokenExpiryTime = expiryTime;
    }

    /// <summary>
    /// Revokes the current refresh token so it can no longer be used to mint
    /// new access tokens. Called on logout to support real token revocation
    /// (stateless JWTs themselves cannot be revoked, but a null refresh token
    /// stops the refresh flow on the next /api/auth/refresh-token call).
    /// </summary>
    public void RevokeRefreshToken()
    {
        RefreshToken = null;
        RefreshTokenExpiryTime = null;
    }

    /// <summary>
    /// Generates a cryptographically-secure 64-hex-char token valid for 15 minutes.
    /// </summary>
    public string RequestPasswordReset()
    {
        PasswordResetToken = Convert
            .ToHexString(System.Security.Cryptography
            .RandomNumberGenerator.GetBytes(32))
            .ToLowerInvariant();

        PasswordResetTokenExpiresAt = DateTime.UtcNow.AddMinutes(15);

        return PasswordResetToken;
    }

    /// <summary>
    /// Returns true only when the supplied token matches and has not expired.
    /// </summary>
    public bool IsPasswordResetTokenValid(string token)
    {
        return PasswordResetToken is not null
            && PasswordResetTokenExpiresAt.HasValue
            && PasswordResetTokenExpiresAt.Value > DateTime.UtcNow
            && string.Equals(
                PasswordResetToken,
                token,
                StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Clears the reset token so it cannot be reused (single-use enforcement).
    /// Password hashing is handled by the caller (AuthService / IPasswordHasher).
    /// </summary>
    public void ClearPasswordResetToken()
    {
        PasswordResetToken          = null;
        PasswordResetTokenExpiresAt = null;
    }

    public void LinkToPatient(Guid patientId)
    {
        PatientId = patientId;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }
}
