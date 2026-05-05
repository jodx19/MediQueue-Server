using System;

namespace MediQueue.Domain.Exceptions;

/// <summary>
/// Exception thrown when a payment amount is insufficient.
/// </summary>
public class InsufficientPaymentException : DomainException
{
    public InsufficientPaymentException(decimal required, decimal provided)
        : base($"Insufficient payment. Required: {required}, Provided: {provided}.", "InsufficientPayment")
    {
    }
}
