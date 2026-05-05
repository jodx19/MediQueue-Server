// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Domain\Entities\Allergy.cs
using System;
using MediQueue.Domain.Common;
using System.Diagnostics.CodeAnalysis;

namespace MediQueue.Domain.Entities;

public enum AllergySeverity
{
    Mild = 1,
    Moderate = 2,
    Severe = 3,
    LifeThreatening = 4
}

/// <summary>
/// Represents a patient's allergy.
/// </summary>
public class Allergy : BaseEntity
{
    /// <summary>
    /// Gets the allergen substance.
    /// </summary>
    public string Allergen { get; private set; }

    /// <summary>
    /// Gets the severity of the allergy.
    /// </summary>
    public AllergySeverity Severity { get; private set; }

    /// <summary>
    /// Gets the reaction description.
    /// </summary>
    public string Reaction { get; private set; }

    /// <summary>
    /// Gets the date the allergy was diagnosed, if known.
    /// </summary>
    public DateOnly? DiagnosedAt { get; private set; }

    private Allergy() 
    { 
        Allergen = null!;
        Reaction = null!;
    }

    internal Allergy(string allergen, AllergySeverity severity, string reaction, DateOnly? diagnosedAt = null)
    {
        Allergen = allergen;
        Severity = severity;
        Reaction = reaction;
        DiagnosedAt = diagnosedAt;
    }
}
