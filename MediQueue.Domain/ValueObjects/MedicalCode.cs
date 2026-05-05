// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Domain\ValueObjects\MedicalCode.cs
using System;

namespace MediQueue.Domain.ValueObjects;

public enum MedicalCodeSystem
{
    ICD10 = 1,
    CPT = 2,
    SNOMED = 3,
    LOINC = 4,
    Custom = 5
}

/// <summary>
/// Represents a medical code (e.g. ICD-10, CPT) as an immutable value object.
/// </summary>
public sealed class MedicalCode : IEquatable<MedicalCode>
{
    /// <summary>
    /// Gets the coding system used.
    /// </summary>
    public MedicalCodeSystem System { get; }

    /// <summary>
    /// Gets the medical code.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Gets the description of the code.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MedicalCode"/> class.
    /// </summary>
    public MedicalCode(MedicalCodeSystem system, string code, string description)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code cannot be empty.", nameof(code));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty.", nameof(description));

        System = system;
        Code = code.Trim();
        Description = description.Trim();
        
        if (!Validate())
        {
            // In a real scenario you might throw a DomainException here, but we will just rely on Validate returning bool
            // Actually, let's let the caller validate or we validate here.
            // The instructions say "Validate() -> bool (format validation per system)".
            // We will just let the object be created, or we can throw. Let's just keep it simple and not throw unless asked.
        }
    }

    /// <summary>
    /// Validates the format of the code per system.
    /// </summary>
    public bool Validate()
    {
        return System switch
        {
            MedicalCodeSystem.ICD10 => ValidateICD10(),
            MedicalCodeSystem.CPT => ValidateCPT(),
            _ => true // Assume custom or others are valid
        };
    }

    private bool ValidateICD10()
    {
        // Basic ICD-10 format check: 3 to 7 characters, first is a letter, second is a number
        if (Code.Length < 3 || Code.Length > 7) return false;
        if (!char.IsLetter(Code[0])) return false;
        if (!char.IsDigit(Code[1])) return false;
        return true;
    }

    private bool ValidateCPT()
    {
        // Basic CPT format check: exactly 5 characters, usually digits, sometimes ending in a letter
        if (Code.Length != 5) return false;
        return true;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is MedicalCode other && Equals(other);

    /// <inheritdoc/>
    public bool Equals(MedicalCode? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        
        return System == other.System &&
               Code == other.Code;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return HashCode.Combine(System, Code);
    }

    public static bool operator ==(MedicalCode? left, MedicalCode? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(MedicalCode? left, MedicalCode? right)
    {
        return !(left == right);
    }
}
