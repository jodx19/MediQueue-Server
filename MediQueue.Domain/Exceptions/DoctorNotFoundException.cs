// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Domain\Exceptions\DoctorNotFoundException.cs
using System;

namespace MediQueue.Domain.Exceptions;

/// <summary>
/// Exception thrown when a doctor cannot be found.
/// </summary>
public class DoctorNotFoundException : DomainException
{
    public DoctorNotFoundException(Guid doctorId)
        : base($"Doctor with ID '{doctorId}' was not found.", "DoctorNotFound")
    {
    }
}
