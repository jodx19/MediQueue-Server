// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Domain\Entities\Payment.cs
using System;
using MediQueue.Domain.Common;
using MediQueue.Domain.Enums;
using MediQueue.Domain.ValueObjects;

namespace MediQueue.Domain.Entities;

/// <summary>
/// Represents a payment made against an invoice.
/// </summary>
public class Payment : BaseEntity
{
    public Money Amount { get; private set; }
    public PaymentMethod Method { get; private set; }
    public DateTime PaidAt { get; private set; }
    public string? ReferenceNumber { get; private set; }
    public string? Notes { get; private set; }

    private Payment() 
    { 
        // For EF Core
        Amount = null!;
    }

    internal Payment(Money amount, PaymentMethod method, string? referenceNumber = null, string? notes = null)
    {
        Amount = amount;
        Method = method;
        PaidAt = DateTime.UtcNow;
        ReferenceNumber = referenceNumber;
        Notes = notes;
    }
}
