// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Domain\Exceptions\DomainException.cs
using System;

namespace MediQueue.Domain.Exceptions;

/// <summary>
/// Represents a base class for all domain-specific exceptions.
/// </summary>
public class DomainException : Exception
{
    /// <summary>
    /// Gets the application specific error code.
    /// </summary>
    public string ErrorCode { get; }

    public DomainException(string message, string errorCode) : base(message)
    {
        ErrorCode = errorCode;
    }
}
