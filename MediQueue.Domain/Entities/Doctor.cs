// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Domain\Entities\Doctor.cs
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
/// Represents a doctor aggregate root in the system.
/// </summary>
public class Doctor : BaseAggregateRoot
{
    private readonly List<WorkingShift> _workingShifts = [];
    private readonly List<DoctorQualification> _qualifications = [];

    /// <summary>Gets the doctor's name.</summary>
    public PersonName PersonName { get; private set; }

    /// <summary>Gets the doctor's specialty.</summary>
    public MedicalSpecialty Specialty { get; private set; }

    /// <summary>Gets the doctor's sub-specialty (optional).</summary>
    public string? SubSpecialty { get; private set; }

    /// <summary>Gets the unique license number.</summary>
    public string LicenseNumber { get; private set; }

    /// <summary>Gets the contact information.</summary>
    public ContactInfo ContactInfo { get; private set; }

    /// <summary>Gets the consultation fee.</summary>
    public Money ConsultationFee { get; private set; }

    /// <summary>Gets the follow-up fee.</summary>
    public Money FollowUpFee { get; private set; }

    /// <summary>Gets the doctor's biography.</summary>
    public string? Bio { get; private set; }

    /// <summary>Gets the years of experience.</summary>
    public int YearsOfExperience { get; private set; }

    /// <summary>Gets a value indicating whether the doctor is currently available.</summary>
    public bool IsAvailable { get; private set; }

    /// <summary>Gets the doctor's working shifts.</summary>
    public IReadOnlyCollection<WorkingShift> WorkingShifts => _workingShifts.AsReadOnly();

    /// <summary>Gets the doctor's qualifications.</summary>
    public IReadOnlyCollection<DoctorQualification> Qualifications => _qualifications.AsReadOnly();

    private Doctor()
    {
        // For EF Core
        PersonName = null!;
        LicenseNumber = null!;
        ContactInfo = null!;
        ConsultationFee = null!;
        FollowUpFee = null!;
    }

    private Doctor(
        PersonName personName,
        MedicalSpecialty specialty,
        string licenseNumber,
        ContactInfo contactInfo,
        Money consultationFee,
        Money followUpFee,
        string? subSpecialty = null,
        string? bio = null,
        int yearsOfExperience = 0)
    {
        PersonName = personName;
        Specialty = specialty;
        LicenseNumber = licenseNumber;
        ContactInfo = contactInfo;
        ConsultationFee = consultationFee;
        FollowUpFee = followUpFee;
        SubSpecialty = subSpecialty;
        Bio = bio;
        YearsOfExperience = yearsOfExperience;
        IsAvailable = true;
    }

    /// <summary>Factory method to create a new doctor.</summary>
    public static Doctor Create(
        PersonName personName,
        MedicalSpecialty specialty,
        string licenseNumber,
        ContactInfo contactInfo,
        Money consultationFee,
        Money followUpFee,
        string? subSpecialty = null,
        string? bio = null,
        int yearsOfExperience = 0)
    {
        var doctor = new Doctor(
            personName, specialty, licenseNumber, contactInfo,
            consultationFee, followUpFee, subSpecialty, bio, yearsOfExperience);

        doctor.AddDomainEvent(new DoctorCreatedEvent(
            doctor.Id, doctor.PersonName.FullName, doctor.Specialty, DateTime.UtcNow));

        return doctor;
    }

    /// <summary>
    /// Adds a working shift for the doctor.
    /// Throws <see cref="DomainException"/> if a shift for the same day already exists
    /// and the time ranges overlap.
    /// </summary>
    public void AddWorkingShift(WorkingShift shift)
    {
        if (shift == null) throw new ArgumentNullException(nameof(shift));

        // Validate: no overlapping shift on the same day
        var existingOnSameDay = _workingShifts
            .Where(s => s.DayOfWeek == shift.DayOfWeek)
            .ToList();

        foreach (var existing in existingOnSameDay)
        {
            // Overlap condition: newStart < existingEnd AND newEnd > existingStart
            bool overlaps = shift.StartTime < existing.EndTime
                         && shift.EndTime > existing.StartTime;

            if (overlaps)
            {
                throw new DomainException(
                    $"A working shift on {shift.DayOfWeek} from {existing.StartTime} to {existing.EndTime} already exists and overlaps with the requested shift.",
                    "WorkingShiftOverlap");
            }
        }

        _workingShifts.Add(shift);
        SetUpdated();
    }

    /// <summary>Removes all working shifts for a given day of week.</summary>
    public void RemoveWorkingShift(DayOfWeek dayOfWeek)
    {
        _workingShifts.RemoveAll(s => s.DayOfWeek == dayOfWeek);
        SetUpdated();
    }

    /// <summary>Updates the consultation and follow-up fees.</summary>
    public void UpdateFees(Money consultationFee, Money followUpFee)
    {
        ConsultationFee = consultationFee ?? throw new ArgumentNullException(nameof(consultationFee));
        FollowUpFee = followUpFee ?? throw new ArgumentNullException(nameof(followUpFee));
        SetUpdated();
    }

    /// <summary>Sets the doctor as unavailable and raises a domain event.</summary>
    public void SetUnavailable(string reason)
    {
        if (!IsAvailable) return;

        IsAvailable = false;
        SetUpdated();

        AddDomainEvent(new DoctorUnavailableEvent(Id, reason, DateTime.UtcNow));
    }

    /// <summary>Adds a qualification to the doctor's profile.</summary>
    public void AddQualification(DoctorQualification qualification)
    {
        if (qualification == null) throw new ArgumentNullException(nameof(qualification));
        _qualifications.Add(qualification);
        SetUpdated();
    }

    /// <summary>
    /// Checks if a given date and time is within the doctor's working shifts.
    /// </summary>
    public bool IsWithinWorkingHours(DateTime scheduledAt, int durationMinutes)
    {
        var day = scheduledAt.DayOfWeek;
        var start = TimeOnly.FromDateTime(scheduledAt);
        var end = start.AddMinutes(durationMinutes);

        return _workingShifts
            .Any(s => s.DayOfWeek == day && s.StartTime <= start && s.EndTime >= end);
    }
}
