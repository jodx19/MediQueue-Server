using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MediQueue.Domain.Common;
using MediQueue.Domain.Events;
using MediQueue.Domain.ValueObjects;

namespace MediQueue.Domain.Entities;

/// <summary>
/// Represents a clinical note for an appointment.
/// </summary>
public sealed class ClinicalNote : BaseAggregateRoot
{
    /// <summary>Gets the appointment identifier.</summary>
    public Guid AppointmentId { get; private set; }

    /// <summary>Gets the doctor identifier.</summary>
    public Guid DoctorId { get; private set; }

    /// <summary>Gets the patient identifier.</summary>
    public Guid PatientId { get; private set; }

    /// <summary>Gets the subjective note (SOAP).</summary>
    public string SubjectiveNote { get; private set; }

    /// <summary>Gets the objective note (SOAP).</summary>
    public string ObjectiveNote { get; private set; }

    /// <summary>Gets the assessment note (SOAP).</summary>
    public string AssessmentNote { get; private set; }

    /// <summary>Gets the plan note (SOAP).</summary>
    public string PlanNote { get; private set; }

    private readonly List<string> _cdtProcedureCodes = [];
    /// <summary>Gets the CDT procedure codes.</summary>
    public IReadOnlyCollection<string> CDTProcedureCodes => _cdtProcedureCodes.AsReadOnly();

    private readonly List<PrescriptionItem> _prescriptions = [];
    /// <summary>Gets the prescriptions.</summary>
    public IReadOnlyCollection<PrescriptionItem> Prescriptions => _prescriptions.AsReadOnly();

    private ClinicalNote(
        Guid appointmentId, 
        Guid doctorId, 
        Guid patientId, 
        string subjectiveNote, 
        string objectiveNote, 
        string assessmentNote, 
        string planNote,
        IEnumerable<string> cdtProcedureCodes,
        IEnumerable<PrescriptionItem> prescriptions)
    {
        AppointmentId = appointmentId;
        DoctorId = doctorId;
        PatientId = patientId;
        SubjectiveNote = subjectiveNote;
        ObjectiveNote = objectiveNote;
        AssessmentNote = assessmentNote;
        PlanNote = planNote;

        foreach (var code in cdtProcedureCodes)
        {
            if (!Regex.IsMatch(code, @"^D\d{4}$"))
                throw new ArgumentException($"Invalid CDT procedure code format: {code}. Must be D followed by 4 digits.");
            _cdtProcedureCodes.Add(code);
        }

        _prescriptions.AddRange(prescriptions);
    }

    /// <summary>
    /// Creates a new clinical note.
    /// </summary>
    /// <param name="appointmentId">The appointment ID.</param>
    /// <param name="doctorId">The doctor ID.</param>
    /// <param name="patientId">The patient ID.</param>
    /// <param name="subjectiveNote">The subjective note.</param>
    /// <param name="objectiveNote">The objective note.</param>
    /// <param name="assessmentNote">The assessment note.</param>
    /// <param name="planNote">The plan note.</param>
    /// <param name="cdtProcedureCodes">The CDT procedure codes.</param>
    /// <param name="prescriptions">The prescriptions.</param>
    /// <returns>A new <see cref="ClinicalNote"/> instance.</returns>
    public static ClinicalNote Create(
        Guid appointmentId, 
        Guid doctorId, 
        Guid patientId, 
        string subjectiveNote, 
        string objectiveNote, 
        string assessmentNote, 
        string planNote,
        IEnumerable<string> cdtProcedureCodes,
        IEnumerable<PrescriptionItem> prescriptions)
    {
        var note = new ClinicalNote(
            appointmentId, 
            doctorId, 
            patientId, 
            subjectiveNote, 
            objectiveNote, 
            assessmentNote, 
            planNote, 
            cdtProcedureCodes, 
            prescriptions);

        note.AddDomainEvent(new ClinicalNoteCreatedEvent(note.Id, appointmentId, patientId, DateTime.UtcNow));
        return note;
    }
}
