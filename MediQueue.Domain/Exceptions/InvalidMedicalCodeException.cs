// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Domain\Exceptions\InvalidMedicalCodeException.cs
using System;

namespace MediQueue.Domain.Exceptions;

/// <summary>
/// Exception thrown when a medical code is invalid for a given system.
/// </summary>
public class InvalidMedicalCodeException : DomainException
{
    public InvalidMedicalCodeException(string code, string system)
        : base($"Medical code '{code}' is invalid for the system '{system}'.", "InvalidMedicalCode")
    {
    }
}
