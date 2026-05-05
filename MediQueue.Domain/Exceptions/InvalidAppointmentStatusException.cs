// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Domain\Exceptions\InvalidAppointmentStatusException.cs
using System;

namespace MediQueue.Domain.Exceptions;

/// <summary>
/// Exception thrown when an invalid appointment status transition is attempted.
/// </summary>
public class InvalidAppointmentStatusException : DomainException
{
    public InvalidAppointmentStatusException(string currentStatus, string attemptedAction)
        : base($"Cannot perform action '{attemptedAction}' when appointment is in status '{currentStatus}'.", "InvalidAppointmentStatus")
    {
    }
}
