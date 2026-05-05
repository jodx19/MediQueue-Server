// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Domain\Entities\Invoice.cs
using System;
using System.Collections.Generic;
using System.Linq;
using MediQueue.Domain.Common;
using MediQueue.Domain.Enums;
using MediQueue.Domain.Events;
using MediQueue.Domain.Exceptions;
using MediQueue.Domain.ValueObjects;

namespace MediQueue.Domain.Entities;

/// <summary>
/// Represents an invoice aggregate root.
/// </summary>
public class Invoice : BaseAggregateRoot
{
    private readonly List<InvoiceItem> _items = [];
    private readonly List<Payment> _payments = [];

    public Guid PatientId { get; private set; }
    public Guid? AppointmentId { get; private set; }
    public string InvoiceNumber { get; private set; }
    public DateTime IssuedAt { get; private set; }
    public DateOnly DueDate { get; private set; }
    public InvoiceStatus Status { get; private set; }
    public Money SubTotal { get; private set; }
    public Money DiscountAmount { get; private set; }
    public Money TaxAmount { get; private set; }

    public Money TotalAmount => SubTotal.Subtract(DiscountAmount).Add(TaxAmount);

    public Money PaidAmount { get; private set; }

    public Money RemainingAmount => TotalAmount.Subtract(PaidAmount);

    public IReadOnlyCollection<InvoiceItem> Items => _items.AsReadOnly();
    public IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();

    private Invoice() 
    { 
        // For EF Core
        InvoiceNumber = null!;
        SubTotal = null!;
        DiscountAmount = null!;
        TaxAmount = null!;
        PaidAmount = null!;
    }

    private Invoice(Guid patientId, Guid? appointmentId, DateOnly dueDate, string defaultCurrency = "EGP")
    {
        PatientId = patientId;
        AppointmentId = appointmentId;
        InvoiceNumber = GenerateInvoiceNumber();
        IssuedAt = DateTime.UtcNow;
        DueDate = dueDate;
        Status = InvoiceStatus.Draft;
        
        SubTotal = new Money(0, defaultCurrency);
        DiscountAmount = new Money(0, defaultCurrency);
        TaxAmount = new Money(0, defaultCurrency);
        PaidAmount = new Money(0, defaultCurrency);
    }

    public static Invoice Create(Guid patientId, Guid? appointmentId, DateOnly dueDate, string defaultCurrency = "EGP")
    {
        var invoice = new Invoice(patientId, appointmentId, dueDate, defaultCurrency);
        
        // Raising InvoiceCreatedEvent here would result in 0 TotalAmount since items aren't added yet.
        // It might be better to raise it when it's Issued, or we can raise it here.
        invoice.AddDomainEvent(new InvoiceCreatedEvent(
            invoice.Id,
            patientId,
            invoice.TotalAmount,
            DateTime.UtcNow));

        return invoice;
    }

    public void AddItem(string description, int quantity, Money unitPrice)
    {
        if (Status != InvoiceStatus.Draft)
            throw new InvalidOperationException("Can only add items to draft invoices.");

        var item = new InvoiceItem(description, quantity, unitPrice);
        _items.Add(item);

        RecalculateTotals();
        SetUpdated();
    }

    public void ApplyDiscount(Money amount)
    {
        if (Status != InvoiceStatus.Draft)
            throw new InvalidOperationException("Can only apply discount to draft invoices.");

        if (amount.Amount > SubTotal.Amount)
            throw new DomainException("Discount cannot exceed subtotal amount.", "DiscountExceedsSubtotal");

        DiscountAmount = amount;
        RecalculateTotals();
        SetUpdated();
    }

    public void ApplyTax(Money amount)
    {
        if (Status != InvoiceStatus.Draft)
            throw new InvalidOperationException("Can only apply tax to draft invoices.");

        TaxAmount = amount;
        RecalculateTotals();
        SetUpdated();
    }

    public void Issue()
    {
        if (Status != InvoiceStatus.Draft)
            throw new InvalidOperationException("Can only issue draft invoices.");

        Status = InvoiceStatus.Issued;
        SetUpdated();
    }

    public void RecordPayment(Money amount, PaymentMethod method, string? referenceNumber = null, string? notes = null)
    {
        if (Status == InvoiceStatus.Paid || Status == InvoiceStatus.Cancelled)
            throw new InvalidOperationException($"Cannot record payment for invoice with status {Status}.");

        if (amount.Amount <= 0)
            throw new ArgumentException("Payment amount must be greater than zero.");

        if (amount > RemainingAmount)
            throw new ArgumentException("Payment amount exceeds remaining amount.");

        var payment = new Payment(amount, method, referenceNumber, notes);
        _payments.Add(payment);

        PaidAmount = PaidAmount.Add(amount);

        if (PaidAmount >= TotalAmount)
        {
            Status = InvoiceStatus.Paid;
            AddDomainEvent(new InvoicePaidEvent(Id, PatientId, DateTime.UtcNow));
        }
        else
        {
            Status = InvoiceStatus.PartiallyPaid;
        }

        SetUpdated();

        AddDomainEvent(new PaymentRecordedEvent(Id, amount, method, DateTime.UtcNow));
    }

    public void Cancel()
    {
        if (Status == InvoiceStatus.Paid)
            throw new InvalidOperationException("Cannot cancel a paid invoice.");

        Status = InvoiceStatus.Cancelled;
        SetUpdated();

        // Could raise an InvoiceCancelledEvent here if needed
    }

    private void RecalculateTotals()
    {
        if (_items.Count == 0) return;

        var currency = _items.First().UnitPrice.Currency;
        var newSubTotal = new Money(_items.Sum(i => i.TotalPrice.Amount), currency);
        SubTotal = newSubTotal;
    }

    private string GenerateInvoiceNumber()
    {
        // INV-YYYYMMDD-XXXX
        var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
        var randomPart = Guid.NewGuid().ToString()[..4].ToUpperInvariant();
        return $"INV-{datePart}-{randomPart}";
    }
}
