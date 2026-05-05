// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Domain\ValueObjects\PrescriptionItem.cs
using System;

namespace MediQueue.Domain.ValueObjects;

/// <summary>
/// Represents a single item in a prescription as an immutable value object.
/// </summary>
public sealed class PrescriptionItem : IEquatable<PrescriptionItem>
{
    public string MedicationName { get; }
    public string? GenericName { get; }
    public string Dosage { get; }
    public string Form { get; }
    public string Frequency { get; }
    public string Duration { get; }
    public int Quantity { get; }
    public string? Instructions { get; }
    public int Refills { get; }

    public PrescriptionItem(
        string medicationName, 
        string dosage, 
        string form, 
        string frequency, 
        string duration, 
        int quantity, 
        string? genericName = null, 
        string? instructions = null, 
        int refills = 0)
    {
        if (string.IsNullOrWhiteSpace(medicationName))
            throw new ArgumentException("Medication name cannot be empty.", nameof(medicationName));

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

        if (refills < 0)
            throw new ArgumentException("Refills cannot be negative.", nameof(refills));

        MedicationName = medicationName.Trim();
        GenericName = genericName?.Trim();
        Dosage = dosage.Trim();
        Form = form.Trim();
        Frequency = frequency.Trim();
        Duration = duration.Trim();
        Quantity = quantity;
        Instructions = instructions?.Trim();
        Refills = refills;
    }

    private PrescriptionItem() { } // For EF Core

    public override bool Equals(object? obj) => obj is PrescriptionItem other && Equals(other);

    public bool Equals(PrescriptionItem? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        
        return MedicationName == other.MedicationName &&
               GenericName == other.GenericName &&
               Dosage == other.Dosage &&
               Form == other.Form &&
               Frequency == other.Frequency &&
               Duration == other.Duration &&
               Quantity == other.Quantity &&
               Instructions == other.Instructions &&
               Refills == other.Refills;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(MedicationName);
        hash.Add(GenericName);
        hash.Add(Dosage);
        hash.Add(Form);
        hash.Add(Frequency);
        hash.Add(Duration);
        hash.Add(Quantity);
        hash.Add(Instructions);
        hash.Add(Refills);
        return hash.ToHashCode();
    }

    public static bool operator ==(PrescriptionItem? left, PrescriptionItem? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(PrescriptionItem? left, PrescriptionItem? right)
    {
        return !(left == right);
    }
}
