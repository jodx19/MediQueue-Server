// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Domain\Entities\Prescription.cs
using System;
using System.Collections.Generic;
using System.Linq;
using MediQueue.Domain.Common;
using MediQueue.Domain.Enums;
using MediQueue.Domain.ValueObjects;

namespace MediQueue.Domain.Entities;

/// <summary>
/// Represents a prescription issued during a clinical visit.
/// </summary>
public class Prescription : BaseEntity
{
    private readonly List<PrescriptionItem> _items = [];

    public string PrescriptionNumber { get; private set; }
    public DateTime IssuedAt { get; private set; }
    public PrescriptionStatus Status { get; private set; }
    public DateOnly ValidUntil { get; private set; }

    public IReadOnlyCollection<PrescriptionItem> Items => _items.AsReadOnly();

    private Prescription() 
    { 
        // For EF Core
        PrescriptionNumber = null!;
    }

    internal Prescription(List<PrescriptionItem> items, DateTime? validUntil = null)
    {
        if (items == null || items.Count == 0)
            throw new ArgumentException("A prescription must have at least one item.");

        PrescriptionNumber = GeneratePrescriptionNumber();
        IssuedAt = DateTime.UtcNow;
        Status = PrescriptionStatus.Active;
        ValidUntil = validUntil.HasValue ? DateOnly.FromDateTime(validUntil.Value) : DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1));
        
        _items.AddRange(items);
    }

    public void Complete()
    {
        Status = PrescriptionStatus.Completed;
        SetUpdated();
    }

    public void Cancel()
    {
        Status = PrescriptionStatus.Cancelled;
        SetUpdated();
    }

    public void PutOnHold()
    {
        Status = PrescriptionStatus.OnHold;
        SetUpdated();
    }

    private string GeneratePrescriptionNumber()
    {
        // RX-YYYYMMDD-XXXX
        var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
        var randomPart = Guid.NewGuid().ToString()[..4].ToUpperInvariant();
        return $"RX-{datePart}-{randomPart}";
    }
}
