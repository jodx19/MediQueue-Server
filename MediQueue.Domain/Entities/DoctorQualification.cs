// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Domain\Entities\DoctorQualification.cs
using System;
using MediQueue.Domain.Common;
using System.Diagnostics.CodeAnalysis;

namespace MediQueue.Domain.Entities;

/// <summary>
/// Represents a doctor's qualification or degree.
/// </summary>
public class DoctorQualification : BaseEntity
{
    /// <summary>
    /// Gets the degree obtained (e.g. "MD", "MRCP").
    /// </summary>
    public required string Degree { get; set; }

    /// <summary>
    /// Gets the institution where the degree was obtained.
    /// </summary>
    public required string Institution { get; set; }

    /// <summary>
    /// Gets the year the degree was obtained.
    /// </summary>
    public int Year { get; private set; }

    private DoctorQualification() { } // For EF Core

    [SetsRequiredMembers]
    internal DoctorQualification(string degree, string institution, int year)
    {
        Degree = degree;
        Institution = institution;
        Year = year;
    }
}
