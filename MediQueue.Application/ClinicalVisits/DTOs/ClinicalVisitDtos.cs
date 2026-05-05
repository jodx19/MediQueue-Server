// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\ClinicalVisits\DTOs\ClinicalVisitDtos.cs
using System;
using System.Collections.Generic;

namespace MediQueue.Application.ClinicalVisits.DTOs;

public class ClinicalVisitDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Guid DoctorId { get; set; }
    public Guid AppointmentId { get; set; }
    public DateTime VisitDate { get; set; }
    public bool IsFinalized { get; set; }
}

public class ClinicalVisitDetailDto : ClinicalVisitDto
{
    public string? SubjectiveNote { get; set; }
    public string? ObjectiveNote { get; set; }
    public string? AssessmentNote { get; set; }
    public string? PlanNote { get; set; }
    
    public List<VitalSignDto> VitalSigns { get; set; } = [];
    public List<DiagnosisDto> Diagnoses { get; set; } = [];
    public List<MedicalProcedureDto> Procedures { get; set; } = [];
    public List<LabRequestDto> LabRequests { get; set; } = [];
    public List<ImagingRequestDto> ImagingRequests { get; set; } = [];
    public List<ReferralDto> Referrals { get; set; } = [];
    public PrescriptionDto? Prescription { get; set; }
}

public class ClinicalVisitSummaryDto : ClinicalVisitDto
{
    public string? AssessmentNote { get; set; }
    public string? PlanNote { get; set; }
    public List<DiagnosisDto> Diagnoses { get; set; } = [];
}

public class VitalSignDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public DateTime RecordedAt { get; set; }
}

public class DiagnosisDto
{
    public Guid Id { get; set; }
    public string ICD10Code { get; set; } = string.Empty;
    public string CodeDescription { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class MedicalProcedureDto
{
    public Guid Id { get; set; }
    public string CPTCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Fee { get; set; }
    public DateTime PerformedAt { get; set; }
}

public class LabRequestDto
{
    public Guid Id { get; set; }
    public string TestName { get; set; } = string.Empty;
    public string? Instructions { get; set; }
    public DateTime RequestedAt { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class ImagingRequestDto
{
    public Guid Id { get; set; }
    public string ImagingType { get; set; } = string.Empty;
    public string BodyPart { get; set; } = string.Empty;
    public string? Instructions { get; set; }
    public DateTime RequestedAt { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class ReferralDto
{
    public Guid Id { get; set; }
    public string ReferredToSpecialty { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string Urgency { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class PrescriptionItemDto
{
    public string MedicationName { get; set; } = string.Empty;
    public string? GenericName { get; set; }
    public string Dosage { get; set; } = string.Empty;
    public string Form { get; set; } = "Tablet";
    public string Frequency { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public string? Instructions { get; set; }
    public int Refills { get; set; } = 0;
}

public class PrescriptionDto
{
    public Guid Id { get; set; }
    public string PrescriptionNumber { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; }
    public DateTime ValidUntil { get; set; }
    public List<PrescriptionItemDto> Items { get; set; } = [];
}
