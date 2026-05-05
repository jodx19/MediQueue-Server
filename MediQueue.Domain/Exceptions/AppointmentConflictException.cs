// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Domain\Exceptions\AppointmentConflictException.cs
using System;

namespace MediQueue.Domain.Exceptions;

/// <summary>
/// Exception thrown when an appointment scheduling conflicts with another appointment.
/// </summary>
public class AppointmentConflictException : DomainException
{
    public AppointmentConflictException(Guid doctorId, DateTime requestedDateTime)
        : base($"Doctor with ID {doctorId} already has an appointment at {requestedDateTime}.", "AppointmentConflict")
    {
    }
}
