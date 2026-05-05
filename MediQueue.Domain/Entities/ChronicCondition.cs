// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Domain\Entities\ChronicCondition.cs
using System;
using MediQueue.Domain.Common;
using System.Diagnostics.CodeAnalysis;

namespace MediQueue.Domain.Entities;

/// <summary>
/// Represents a patient's chronic condition.
/// </summary>
public class ChronicCondition : BaseEntity
{
    /// <summary>
    /// Gets the name of the condition.
    /// </summary>
    public required string ConditionName { get; set; }

    /// <summary>
    /// Gets the ICD-10 code for the condition.
    /// </summary>
    public string? ICD10Code { get; private set; }

    /// <summary>
    /// Gets the date the condition was diagnosed.
    /// </summary>
    public DateOnly? DiagnosedAt { get; private set; }

    /// <summary>
    /// Gets additional notes about the condition.
    /// </summary>
    public string? Notes { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the condition is currently active.
    /// </summary>
    public bool IsActive { get; private set; }

    private ChronicCondition() { } // For EF Core

    [SetsRequiredMembers]
    internal ChronicCondition(string conditionName, string? icd10Code = null, DateOnly? diagnosedAt = null, string? notes = null)
    {
        ConditionName = conditionName;
        ICD10Code = icd10Code;
        DiagnosedAt = diagnosedAt;
        Notes = notes;
        IsActive = true;
    }

    /// <summary>
    /// Marks the condition as resolved or inactive.
    /// </summary>
    public void MarkAsInactive()
    {
        IsActive = false;
        SetUpdated();
    }
}
