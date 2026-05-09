// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Domain\ValueObjects\WorkingShift.cs
using System;
using System.Collections.Generic;

namespace MediQueue.Domain.ValueObjects;

/// <summary>
/// Represents a working shift for a doctor as an immutable value object.
/// </summary>
public sealed class WorkingShift : IEquatable<WorkingShift>
{
    /// <summary>
    /// Gets the day of the week for this shift.
    /// </summary>
    public DayOfWeek DayOfWeek { get; }

    /// <summary>
    /// Gets the start time of the shift.
    /// </summary>
    public TimeOnly StartTime { get; }

    /// <summary>
    /// Gets the end time of the shift.
    /// </summary>
    public TimeOnly EndTime { get; }

    /// <summary>
    /// Gets the duration of each slot in minutes. Default is 20.
    /// </summary>
    public int SlotDurationMinutes { get; }

    /// <summary>
    /// Gets the maximum number of patients allowed per slot. Default is 1.
    /// </summary>
    public int MaxPatientsPerSlot { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkingShift"/> class.
    /// </summary>
    public WorkingShift(
        DayOfWeek dayOfWeek, 
        TimeOnly startTime, 
        TimeOnly endTime, 
        int slotDurationMinutes = 20, 
        int maxPatientsPerSlot = 1)
    {
        if (startTime >= endTime)
            throw new ArgumentException("Start time must be before end time.");

        if (slotDurationMinutes <= 0)
            throw new ArgumentException("Slot duration must be greater than zero.", nameof(slotDurationMinutes));
            
        if (maxPatientsPerSlot <= 0)
            throw new ArgumentException("Max patients per slot must be greater than zero.", nameof(maxPatientsPerSlot));

        DayOfWeek = dayOfWeek;
        StartTime = startTime;
        EndTime = endTime;
        SlotDurationMinutes = slotDurationMinutes;
        MaxPatientsPerSlot = maxPatientsPerSlot;
    }

    private WorkingShift() { } // For EF Core

    /// <summary>
    /// Generates the available time slots for this shift based on the slot duration.
    /// </summary>
    public List<TimeOnly> GenerateSlots()
    {
        var slots = new List<TimeOnly>();
        var currentSlot = StartTime;
        while (currentSlot < EndTime)
        {
            var nextSlot = currentSlot.AddMinutes(SlotDurationMinutes);
            if (nextSlot <= EndTime)
            {
                slots.Add(currentSlot);
            }
            currentSlot = nextSlot;
        }

        return slots;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is WorkingShift other && Equals(other);

    /// <inheritdoc/>
    public bool Equals(WorkingShift? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        
        return DayOfWeek == other.DayOfWeek &&
               StartTime == other.StartTime &&
               EndTime == other.EndTime &&
               SlotDurationMinutes == other.SlotDurationMinutes &&
               MaxPatientsPerSlot == other.MaxPatientsPerSlot;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return HashCode.Combine(DayOfWeek, StartTime, EndTime, SlotDurationMinutes, MaxPatientsPerSlot);
    }

    public static bool operator ==(WorkingShift? left, WorkingShift? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(WorkingShift? left, WorkingShift? right)
    {
        return !(left == right);
    }
}
