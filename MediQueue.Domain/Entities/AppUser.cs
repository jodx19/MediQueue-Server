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
    public string? PasswordResetToken { get; private set; }
    public DateTime? PasswordResetTokenExpiresAt { get; private set; }
    
    // Email Verification Fields (Already in DB)
    public bool EmailConfirmed { get; private set; }
    public string? EmailVerificationToken { get; private set; }
    public DateTime? EmailVerificationTokenExpiresAt { get; private set; }

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
        EmailConfirmed = false;
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

    public void GeneratePasswordResetToken(string token, TimeSpan validFor)
    {
        PasswordResetToken = token;
        PasswordResetTokenExpiresAt = DateTime.UtcNow.Add(validFor);
    }

    public void ResetPassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        PasswordResetToken = null;
        PasswordResetTokenExpiresAt = null;
        
        // Revoke all existing sessions on password reset for security
        RevokeRefreshToken();
    }

    /// <summary>
    /// Generates a secure email verification token that expires in 24 hours.
    /// Called after registration to send a verification link to the user's email.
    /// </summary>
    public void GenerateEmailVerificationToken(string token)
    {
        EmailVerificationToken = token;
        EmailVerificationTokenExpiresAt = DateTime.UtcNow.AddHours(24);
        EmailConfirmed = false;
    }

    /// <summary>
    /// Confirms the user's email address when they click the verification link.
    /// Returns false if the token is invalid or expired.
    /// </summary>
    public bool ConfirmEmailVerification(string token)
    {
        if (string.IsNullOrEmpty(EmailVerificationToken)) return false;
        if (EmailVerificationToken != token) return false;
        if (EmailVerificationTokenExpiresAt.HasValue && EmailVerificationTokenExpiresAt < DateTime.UtcNow) return false;

        EmailConfirmed = true;
        EmailVerificationToken = null;
        EmailVerificationTokenExpiresAt = null;
        return true;
    }
}
