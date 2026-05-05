using System;
using System.Collections.Generic;

namespace MediQueue.Domain.ValueObjects;

/// <summary>
/// Represents a working hour slot for a doctor.
/// </summary>
/// <param name="DayOfWeek">The day of the week.</param>
/// <param name="StartTime">The start time.</param>
/// <param name="EndTime">The end time.</param>
/// <param name="SlotDurationMinutes">The duration of each slot in minutes. Default is 30.</param>
public sealed class WorkingHourSlot(DayOfWeek DayOfWeek, TimeSpan StartTime, TimeSpan EndTime, int SlotDurationMinutes = 30)
{
    /// <summary>
    /// Gets the day of the week.
    /// </summary>
    public DayOfWeek DayOfWeek { get; } = DayOfWeek;

    /// <summary>
    /// Gets the start time.
    /// </summary>
    public TimeSpan StartTime { get; } = StartTime < EndTime ? StartTime : throw new ArgumentException("Start time must be before end time.");

    /// <summary>
    /// Gets the end time.
    /// </summary>
    public TimeSpan EndTime { get; } = EndTime;

    /// <summary>
    /// Gets the slot duration in minutes.
    /// </summary>
    public int SlotDurationMinutes { get; } = SlotDurationMinutes > 0 ? SlotDurationMinutes : throw new ArgumentException("Slot duration must be positive.");

    /// <summary>
    /// Generates time slots based on the start time, end time, and slot duration.
    /// </summary>
    /// <returns>A collection of generated time slots.</returns>
    public IEnumerable<TimeSpan> GenerateSlots()
    {
        var current = StartTime;
        var duration = TimeSpan.FromMinutes(SlotDurationMinutes);

        while (current + duration <= EndTime)
        {
            yield return current;
            current += duration;
        }
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (obj is not WorkingHourSlot other)
            return false;

        return DayOfWeek == other.DayOfWeek &&
               StartTime == other.StartTime &&
               EndTime == other.EndTime &&
               SlotDurationMinutes == other.SlotDurationMinutes;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return HashCode.Combine(DayOfWeek, StartTime, EndTime, SlotDurationMinutes);
    }
}
