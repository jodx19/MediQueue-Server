using System;

namespace MediQueue.Application.Interfaces;

public interface ICurrentUserService
{
    string? UserId { get; }
    string? Username { get; }
    string? Role { get; }
    bool IsAuthenticated { get; }
    Guid? DoctorId { get; }
    Guid? PatientId { get; }
}
