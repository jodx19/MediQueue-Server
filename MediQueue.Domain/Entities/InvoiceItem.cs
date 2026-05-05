// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Domain\Entities\InvoiceItem.cs
using System;
using MediQueue.Domain.Common;
using MediQueue.Domain.ValueObjects;

namespace MediQueue.Domain.Entities;

/// <summary>
/// Represents a single item in an invoice.
/// </summary>
public class InvoiceItem : BaseEntity
{
    public string Description { get; private set; }
    public int Quantity { get; private set; }
    public Money UnitPrice { get; private set; }

    public Money TotalPrice => UnitPrice.Multiply(Quantity);

    private InvoiceItem() 
    { 
        // For EF Core
        Description = null!;
        UnitPrice = null!;
    }

    internal InvoiceItem(string description, int quantity, Money unitPrice)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty.", nameof(description));
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

        Description = description.Trim();
        Quantity = quantity;
        UnitPrice = unitPrice;
    }
}
