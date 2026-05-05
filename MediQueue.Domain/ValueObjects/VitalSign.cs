// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Domain\ValueObjects\VitalSign.cs
using System;
using MediQueue.Domain.Enums;

namespace MediQueue.Domain.ValueObjects;

/// <summary>
/// Represents a vital sign measurement as an immutable value object.
/// </summary>
public sealed class VitalSign : IEquatable<VitalSign>
{
    /// <summary>
    /// Gets the type of vital sign.
    /// </summary>
    public VitalSignType Type { get; }

    /// <summary>
    /// Gets the measured value.
    /// </summary>
    public decimal Value { get; }

    /// <summary>
    /// Gets the unit of measurement.
    /// </summary>
    public string Unit { get; }

    /// <summary>
    /// Gets the date and time when the measurement was taken.
    /// </summary>
    public DateTime MeasuredAt { get; }

    /// <summary>
    /// Gets a value indicating whether the measurement is outside normal ranges.
    /// </summary>
    public bool IsAbnormal { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="VitalSign"/> class.
    /// </summary>
    public VitalSign(VitalSignType type, decimal value, string unit, DateTime measuredAt)
    {
        if (string.IsNullOrWhiteSpace(unit))
            throw new ArgumentException("Unit cannot be empty.", nameof(unit));

        Type = type;
        Value = value;
        Unit = unit.Trim();
        MeasuredAt = measuredAt;
        
        var (min, max) = GetNormalRange(type);
        IsAbnormal = value < min || value > max;
    }

    private VitalSign() 
    { 
        Unit = null!;
    }

    /// <summary>
    /// Gets the normal range for a specific vital sign type.
    /// </summary>
    public static (decimal min, decimal max) GetNormalRange(VitalSignType type)
    {
        return type switch
        {
            VitalSignType.BloodPressureSystolic => (90m, 120m),
            VitalSignType.BloodPressureDiastolic => (60m, 80m),
            VitalSignType.HeartRate => (60m, 100m),
            VitalSignType.RespiratoryRate => (12m, 20m),
            VitalSignType.Temperature => (36.5m, 37.5m),
            VitalSignType.OxygenSaturation => (95m, 100m),
            // Default ranges for purely informational metrics or metrics that vary wildly by individual
            VitalSignType.Weight => (0m, decimal.MaxValue),
            VitalSignType.Height => (0m, decimal.MaxValue),
            VitalSignType.BMI => (18.5m, 24.9m),
            VitalSignType.BloodGlucose => (70m, 140m),
            VitalSignType.PainScale => (0m, 3m), // Assume 0-3 is mild/normal, higher is abnormal
            _ => (0m, decimal.MaxValue)
        };
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is VitalSign other && Equals(other);

    /// <inheritdoc/>
    public bool Equals(VitalSign? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        
        return Type == other.Type &&
               Value == other.Value &&
               Unit == other.Unit &&
               MeasuredAt == other.MeasuredAt;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return HashCode.Combine(Type, Value, Unit, MeasuredAt);
    }

    public static bool operator ==(VitalSign? left, VitalSign? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(VitalSign? left, VitalSign? right)
    {
        return !(left == right);
    }
}
