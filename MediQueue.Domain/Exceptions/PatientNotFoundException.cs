// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Domain\Exceptions\PatientNotFoundException.cs
using System;

namespace MediQueue.Domain.Exceptions;

/// <summary>
/// Exception thrown when a patient cannot be found.
/// </summary>
public class PatientNotFoundException : DomainException
{
    public PatientNotFoundException(string identifier)
        : base($"Patient with identifier '{identifier}' was not found.", "PatientNotFound")
    {
    }
}
