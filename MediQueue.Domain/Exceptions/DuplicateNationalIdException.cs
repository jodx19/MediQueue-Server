// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Domain\Exceptions\DuplicateNationalIdException.cs
using System;

namespace MediQueue.Domain.Exceptions;

/// <summary>
/// Exception thrown when attempting to register a patient with a national ID that is already in use.
/// </summary>
public class DuplicateNationalIdException : DomainException
{
    public DuplicateNationalIdException(string nationalId)
        : base($"A patient with National ID '{nationalId}' already exists.", "DuplicateNationalId")
    {
    }
}
