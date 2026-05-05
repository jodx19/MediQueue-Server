// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Domain\Entities\MedicalProcedure.cs
using System;
using MediQueue.Domain.Common;
using MediQueue.Domain.ValueObjects;

namespace MediQueue.Domain.Entities;

/// <summary>
/// Represents a medical procedure performed during a clinical visit.
/// </summary>
public class MedicalProcedure : BaseEntity
{
    public MedicalCode MedicalCode { get; private set; }
    public string Description { get; private set; }
    public Money Fee { get; private set; }
    public DateTime PerformedAt { get; private set; }

    private MedicalProcedure() 
    { 
        // For EF Core
        MedicalCode = null!;
        Description = null!;
        Fee = null!;
    }

    internal MedicalProcedure(MedicalCode medicalCode, string description, Money fee, DateTime performedAt)
    {
        MedicalCode = medicalCode;
        Description = description;
        Fee = fee;
        PerformedAt = performedAt;
    }
}
