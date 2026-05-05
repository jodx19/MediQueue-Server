// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Domain\Entities\CurrentMedication.cs
using System;
using MediQueue.Domain.Common;
using System.Diagnostics.CodeAnalysis;

namespace MediQueue.Domain.Entities;

/// <summary>
/// Represents a medication the patient is currently taking.
/// </summary>
public class CurrentMedication : BaseEntity
{
    /// <summary>
    /// Gets the name of the medication.
    /// </summary>
    public required string MedicationName { get; set; }

    /// <summary>
    /// Gets the dosage of the medication.
    /// </summary>
    public required string Dosage { get; set; }

    /// <summary>
    /// Gets the frequency of the medication.
    /// </summary>
    public required string Frequency { get; set; }

    /// <summary>
    /// Gets the date the medication was started.
    /// </summary>
    public DateOnly StartedAt { get; private set; }

    /// <summary>
    /// Gets the name of the prescriber.
    /// </summary>
    public string? PrescribedBy { get; private set; }

    private CurrentMedication() { } // For EF Core

    [SetsRequiredMembers]
    internal CurrentMedication(string medicationName, string dosage, string frequency, DateOnly startedAt, string? prescribedBy = null)
    {
        MedicationName = medicationName;
        Dosage = dosage;
        Frequency = frequency;
        StartedAt = startedAt;
        PrescribedBy = prescribedBy;
    }
}
