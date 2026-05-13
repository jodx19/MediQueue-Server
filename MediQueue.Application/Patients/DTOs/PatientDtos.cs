// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Patients\DTOs\PatientDtos.cs
using System;
using System.Collections.Generic;
using MediQueue.Domain.Enums;

namespace MediQueue.Application.Patients.DTOs;

public class PatientDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public BloodType BloodType { get; set; }
    public string NationalId { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string MedicalRecordNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class PatientDetailDto : PatientDto
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Governorate { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public MaritalStatus MaritalStatus { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? InsuranceProvider { get; set; }
    public string? InsurancePolicyNumber { get; set; }
    
    public List<AllergyDto> Allergies { get; set; } = [];
    public List<ChronicConditionDto> ChronicConditions { get; set; } = [];
    public List<CurrentMedicationDto> CurrentMedications { get; set; } = [];
}

public class PatientSummaryDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    public string MedicalRecordNumber { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public BloodType BloodType { get; set; }
    public DateTime? LastVisitDate { get; set; }
    public bool IsActive { get; set; }
}

public class AllergyDto
{
    public Guid Id { get; set; }
    public string Allergen { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty; // Mapped from Enum
    public string Reaction { get; set; } = string.Empty;
    public DateOnly? DiagnosedAt { get; set; }
}

public class ChronicConditionDto
{
    public Guid Id { get; set; }
    public string ConditionName { get; set; } = string.Empty;
    public string? ICD10Code { get; set; }
    public DateOnly? DiagnosedAt { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
}

public class CurrentMedicationDto
{
    public Guid Id { get; set; }
    public string MedicationName { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public DateOnly StartedAt { get; set; }
    public string? PrescribedBy { get; set; }
}

public class PatientMedicalHistoryDto
{
    public Guid PatientId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string MedicalRecordNumber { get; set; } = string.Empty;
    
    public List<AllergyDto> Allergies { get; set; } = [];
    public List<ChronicConditionDto> ChronicConditions { get; set; } = [];
    public List<CurrentMedicationDto> CurrentMedications { get; set; } = [];
    
    // In a real scenario we'd define a VisitSummaryDto here, but let's use a dynamic or specific one later
    public List<object> LastVisitsSummary { get; set; } = []; 
}
