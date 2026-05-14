// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Domain\Entities\ClinicalVisit.cs
using System;
using System.Collections.Generic;
using System.Linq;
using MediQueue.Domain.Common;
using MediQueue.Domain.Enums;
using MediQueue.Domain.Events;
using MediQueue.Domain.Exceptions;
using MediQueue.Domain.ValueObjects;

namespace MediQueue.Domain.Entities;

/// <summary>
/// Represents a clinical visit aggregate root.
/// </summary>
public class ClinicalVisit : BaseAggregateRoot
{
    private readonly List<VitalSign> _vitalSigns = [];
    private readonly List<Diagnosis> _diagnoses = [];
    private readonly List<MedicalProcedure> _procedures = [];
    private readonly List<LabRequest> _labRequests = [];
    private readonly List<ImagingRequest> _imagingRequests = [];
    private readonly List<Referral> _referrals = [];
    private readonly List<Prescription> _prescriptions = [];

    public Guid AppointmentId { get; private set; }
    public virtual Appointment Appointment { get; private set; } = null!;
    public Guid DoctorId { get; private set; }
    public virtual Doctor Doctor { get; private set; } = null!;
    public Guid PatientId { get; private set; }
    public virtual Patient Patient { get; private set; } = null!;
    public DateTime VisitDate { get; private set; }

    // SOAP Notes
    public string? SubjectiveNote { get; private set; }
    public string? ObjectiveNote { get; private set; }
    public string? AssessmentNote { get; private set; }
    public string? PlanNote { get; private set; }

    /// <summary>Indicates whether this visit has been finalized (locked).</summary>
    public bool IsFinalized { get; private set; }

    // Collections
    public IReadOnlyCollection<VitalSign> VitalSigns => _vitalSigns.AsReadOnly();
    public IReadOnlyCollection<Diagnosis> Diagnoses => _diagnoses.AsReadOnly();
    public IReadOnlyCollection<MedicalProcedure> Procedures => _procedures.AsReadOnly();
    public IReadOnlyCollection<LabRequest> LabRequests => _labRequests.AsReadOnly();
    public IReadOnlyCollection<ImagingRequest> ImagingRequests => _imagingRequests.AsReadOnly();
    public IReadOnlyCollection<Referral> Referrals => _referrals.AsReadOnly();
    public IReadOnlyCollection<Prescription> Prescriptions => _prescriptions.AsReadOnly();

    private ClinicalVisit() { } // For EF Core

    private ClinicalVisit(Guid appointmentId, Guid doctorId, Guid patientId, DateTime visitDate)
    {
        AppointmentId = appointmentId;
        DoctorId = doctorId;
        PatientId = patientId;
        VisitDate = visitDate;
    }

    /// <summary>Factory method to create a new clinical visit.</summary>
    public static ClinicalVisit Create(Guid appointmentId, Guid doctorId, Guid patientId, DateTime visitDate)
    {
        var visit = new ClinicalVisit(appointmentId, doctorId, patientId, visitDate);

        visit.AddDomainEvent(new ClinicalVisitCreatedEvent(
            visit.Id, appointmentId, patientId, DateTime.UtcNow));

        return visit;
    }


    /// <summary>Adds a vital sign measurement to the visit.</summary>
    public void UpdateSOAPNotes(string? subjective, string? objective, string? assessment, string? plan)
    {
        GuardNotFinalized();
        SubjectiveNote = subjective;
        ObjectiveNote = objective;
        AssessmentNote = assessment;
        PlanNote = plan;
        SetUpdated();
    }

    public void AddVitalSign(VitalSign vitalSign)
    {
        if (vitalSign == null) throw new ArgumentNullException(nameof(vitalSign));
        GuardNotFinalized();
        _vitalSigns.Add(vitalSign);
        SetUpdated();
    }

    /// <summary>Adds a diagnosis to the visit.</summary>
    public void AddDiagnosis(MedicalCode medicalCode, string description, DiagnosisType type, string? notes = null)
    {
        GuardNotFinalized();
        _diagnoses.Add(new Diagnosis(medicalCode, description, type, notes));
        SetUpdated();
    }

    /// <summary>Adds a medical procedure performed during the visit.</summary>
    public void AddProcedure(MedicalCode medicalCode, string description, Money fee)
    {
        GuardNotFinalized();
        _procedures.Add(new MedicalProcedure(medicalCode, description, fee, DateTime.UtcNow));
        SetUpdated();
    }

    /// <summary>Adds a lab request to the visit.</summary>
    public void AddLabRequest(string testName, string? instructions = null)
    {
        GuardNotFinalized();
        _labRequests.Add(new LabRequest(testName, instructions));
        SetUpdated();
    }

    /// <summary>Adds an imaging request to the visit.</summary>
    public void AddImagingRequest(ImagingType imagingType, string bodyPart, string? instructions = null)
    {
        GuardNotFinalized();
        _imagingRequests.Add(new ImagingRequest(imagingType, bodyPart, instructions));
        SetUpdated();
    }

    /// <summary>Adds a specialist referral to the visit.</summary>
    public void AddReferral(MedicalSpecialty specialty, string reason, ReferralUrgency urgency,
        string? referredToDoctorName = null, string? notes = null)
    {
        GuardNotFinalized();
        _referrals.Add(new Referral(specialty, reason, urgency, referredToDoctorName, notes));
        SetUpdated();
    }

    /// <summary>Creates a prescription and adds it to the visit. Raises PrescriptionCreatedEvent.</summary>
    public void CreatePrescription(List<PrescriptionItem> items, DateTime? validUntil = null)
    {
        GuardNotFinalized();
        var prescription = new Prescription(items, validUntil);
        _prescriptions.Add(prescription);
        SetUpdated();

        AddDomainEvent(new PrescriptionCreatedEvent(
            prescription.Id, PatientId, Id, DateTime.UtcNow));
    }

    /// <summary>
    /// Finalizes the visit. Validates:
    ///   - All four SOAP sections are non-empty.
    ///   - At least one diagnosis has been added.
    ///   - Visit has not already been finalized.
    /// Raises <see cref="VisitFinalizedEvent"/> on success.
    /// </summary>
    public void FinalizeVisit()
    {
        if (IsFinalized)
            throw new DomainException("Clinical visit has already been finalized.", "VisitAlreadyFinalized");

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(SubjectiveNote))
            errors.Add("Subjective (S) SOAP note is required.");
        if (string.IsNullOrWhiteSpace(ObjectiveNote))
            errors.Add("Objective (O) SOAP note is required.");
        if (string.IsNullOrWhiteSpace(AssessmentNote))
            errors.Add("Assessment (A) SOAP note is required.");
        if (string.IsNullOrWhiteSpace(PlanNote))
            errors.Add("Plan (P) SOAP note is required.");
        if (!_diagnoses.Any())
            errors.Add("At least one diagnosis is required to finalize a visit.");

        if (errors.Any())
            throw new DomainException(string.Join(" | ", errors), "IncompleteVisitData");

        IsFinalized = true;
        SetUpdated();

        AddDomainEvent(new VisitFinalizedEvent(Id, PatientId, DoctorId, DateTime.UtcNow));
    }

    private void GuardNotFinalized()
    {
        if (IsFinalized)
            throw new DomainException("Cannot modify a finalized clinical visit.", "VisitAlreadyFinalized");
    }
}
