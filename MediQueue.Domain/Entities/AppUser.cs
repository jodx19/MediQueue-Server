// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Domain\Entities\AppUser.cs
using System;
using MediQueue.Domain.Common;

namespace MediQueue.Domain.Entities;

public enum UserRole
{
    Admin = 1,
    Doctor = 2,
    Patient = 3,
    Receptionist = 4
}

public class AppUser : AuditableEntity
{
    public string Username { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public UserRole Role { get; private set; }
    public Guid? DoctorId { get; private set; }
    public Guid? PatientId { get; private set; }
    public bool IsActive { get; private set; }
    public string? RefreshToken { get; private set; }
    public DateTime? RefreshTokenExpiryTime { get; private set; }

    private AppUser() 
    { 
        Username = null!;
        Email = null!;
        PasswordHash = null!;
    }

    private AppUser(string username, string email, string passwordHash, UserRole role, Guid? doctorId = null, Guid? patientId = null)
    {
        Username = username;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        DoctorId = doctorId;
        PatientId = patientId;
        IsActive = true;
    }

    public static AppUser Create(string username, string email, string passwordHash, UserRole role, Guid? doctorId = null, Guid? patientId = null)
    {
        return new AppUser(username, email, passwordHash, role, doctorId, patientId);
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

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }
}
