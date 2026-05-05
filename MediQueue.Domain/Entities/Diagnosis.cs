// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Domain\Entities\Diagnosis.cs
using System;
using MediQueue.Domain.Common;
using MediQueue.Domain.Enums;
using MediQueue.Domain.ValueObjects;

namespace MediQueue.Domain.Entities;


/// <summary>
/// Represents a medical diagnosis during a clinical visit.
/// </summary>
public class Diagnosis : BaseEntity
{
    public MedicalCode MedicalCode { get; private set; }
    public string Description { get; private set; }
    public DiagnosisType Type { get; private set; }
    public string? Notes { get; private set; }


    private Diagnosis() 
    { 
        // For EF Core
        MedicalCode = null!;
        Description = null!;
    }

    internal Diagnosis(MedicalCode medicalCode, string description, DiagnosisType type, string? notes = null)
    {
        MedicalCode = medicalCode;
        Description = description;
        Type = type;
        Notes = notes;
    }
}
