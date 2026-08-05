// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Domain\Entities\Patient.cs
using System;
using System.Collections.Generic;
using System.Linq;
using MediQueue.Domain.Common;
using MediQueue.Domain.Enums;
using MediQueue.Domain.Events;
using MediQueue.Domain.ValueObjects;

namespace MediQueue.Domain.Entities;

/// <summary>
/// Represents a patient aggregate root in the system.
/// </summary>
public class Patient : BaseAggregateRoot
{
    private readonly List<Allergy> _allergies = [];
    private readonly List<ChronicCondition> _chronicConditions = [];
    private readonly List<CurrentMedication> _currentMedications = [];

    /// <summary>
    /// Gets the patient's name.
    /// </summary>
    public PersonName PersonName { get; private set; }

    /// <summary>
    /// Gets the patient's date of birth.
    /// </summary>
    public DateOnly DateOfBirth { get; private set; }

    /// <summary>
    /// Gets the patient's gender.
    /// </summary>
    public Gender Gender { get; private set; }

    /// <summary>
    /// Gets the patient's blood type.
    /// </summary>
    public BloodType BloodType { get; private set; }

    /// <summary>
    /// Gets the patient's unique national identifier.
    /// </summary>
    public string NationalId { get; private set; }

    /// <summary>
    /// Gets the patient's contact information.
    /// </summary>
    public ContactInfo ContactInfo { get; private set; }

    /// <summary>
    /// Gets the patient's address.
    /// </summary>
    public Address Address { get; private set; }

    /// <summary>
    /// Gets the patient's marital status.
    /// </summary>
    public MaritalStatus MaritalStatus { get; private set; }

    /// <summary>
    /// Gets the emergency contact name.
    /// </summary>
    public string? EmergencyContactName { get; private set; }

    /// <summary>
    /// Gets the emergency contact phone number.
    /// </summary>
    public string? EmergencyContactPhone { get; private set; }

    /// <summary>
    /// Gets the insurance provider name.
    /// </summary>
    public string? InsuranceProvider { get; private set; }

    /// <summary>
    /// Gets the insurance policy number.
    /// </summary>
    public string? InsurancePolicyNumber { get; private set; }

    /// <summary>
    /// Gets the auto-generated medical record number (MRN).
    /// </summary>
    public string MedicalRecordNumber { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the patient record is active.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Gets the patient's allergies.
    /// </summary>
    public IReadOnlyCollection<Allergy> Allergies => _allergies.AsReadOnly();

    /// <summary>
    /// Gets the patient's chronic conditions.
    /// </summary>
    public IReadOnlyCollection<ChronicCondition> ChronicConditions => _chronicConditions.AsReadOnly();

    /// <summary>
    /// Gets the patient's current medications.
    /// </summary>
    public IReadOnlyCollection<CurrentMedication> CurrentMedications => _currentMedications.AsReadOnly();

    private Patient() 
    { 
        // For EF Core
        PersonName = null!;
        NationalId = null!;
        ContactInfo = null!;
        Address = null!;
        MedicalRecordNumber = null!;
    }

    private Patient(
        PersonName personName,
        DateOnly dateOfBirth,
        Gender gender,
        BloodType bloodType,
        string nationalId,
        ContactInfo contactInfo,
        Address address,
        MaritalStatus maritalStatus,
        string? emergencyContactName,
        string? emergencyContactPhone,
        string? insuranceProvider,
        string? insurancePolicyNumber)
    {
        PersonName = personName;
        DateOfBirth = dateOfBirth;
        Gender = gender;
        BloodType = bloodType;
        NationalId = nationalId;
        ContactInfo = contactInfo;
        Address = address;
        MaritalStatus = maritalStatus;
        EmergencyContactName = emergencyContactName;
        EmergencyContactPhone = emergencyContactPhone;
        InsuranceProvider = insuranceProvider;
        InsurancePolicyNumber = insurancePolicyNumber;
        
        IsActive = true;
        MedicalRecordNumber = GenerateMRN();
    }

    /// <summary>
    /// Factory method to register a new patient.
    /// </summary>
    public static Patient Register(
        PersonName personName,
        DateOnly dateOfBirth,
        Gender gender,
        BloodType bloodType,
        string nationalId,
        ContactInfo contactInfo,
        Address address,
        MaritalStatus maritalStatus,
        string? emergencyContactName = null,
        string? emergencyContactPhone = null,
        string? insuranceProvider = null,
        string? insurancePolicyNumber = null)
    {
        var patient = new Patient(
            personName,
            dateOfBirth,
            gender,
            bloodType,
            nationalId,
            contactInfo,
            address,
            maritalStatus,
            emergencyContactName,
            emergencyContactPhone,
            insuranceProvider,
            insurancePolicyNumber);

        patient.AddDomainEvent(new PatientRegisteredEvent(
            patient.Id,
            patient.PersonName.FullName,
            patient.MedicalRecordNumber,
            DateTime.UtcNow));

        return patient;
    }

    /// <summary>
    /// Updates the contact information.
    /// </summary>
    public void UpdateContactInfo(ContactInfo contactInfo)
    {
        ContactInfo = contactInfo ?? throw new ArgumentNullException(nameof(contactInfo));
        SetUpdated();
    }

    /// <summary>
    /// Adds an allergy to the patient.
    /// </summary>
    public void AddAllergy(string allergen, AllergySeverity severity, string reaction)
    {
        var allergy = new Allergy(allergen, severity, reaction);
        _allergies.Add(allergy);
        SetUpdated();
    }

    /// <summary>
    /// Removes an allergy from the patient by ID.
    /// </summary>
    public void RemoveAllergy(Guid allergyId)
    {
        var allergy = _allergies.FirstOrDefault(a => a.Id == allergyId);
        if (allergy != null)
        {
            _allergies.Remove(allergy);
            SetUpdated();
        }
    }

    /// <summary>
    /// Adds a chronic condition to the patient.
    /// </summary>
    public void AddChronicCondition(string name, DateOnly? diagnosedAt = null, string? notes = null)
    {
        var condition = new ChronicCondition(name, diagnosedAt: diagnosedAt, notes: notes);
        _chronicConditions.Add(condition);
        SetUpdated();
    }

    /// <summary>
    /// Removes a chronic condition from the patient by ID.
    /// </summary>
    public void RemoveChronicCondition(Guid conditionId)
    {
        var condition = _chronicConditions.FirstOrDefault(c => c.Id == conditionId);
        if (condition != null)
        {
            _chronicConditions.Remove(condition);
            SetUpdated();
        }
    }

    /// <summary>
    /// Deactivates the patient record.
    /// </summary>
    public void Deactivate()
    {
        if (!IsActive) return;

        IsActive = false;
        SetUpdated();

        AddDomainEvent(new PatientDeactivatedEvent(Id, DateTime.UtcNow));
    }

    private string GenerateMRN()
    {
        // MRN-YYYYMMDD-XXXX
        var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
        var randomPart = Guid.NewGuid().ToString()[..4].ToUpperInvariant();
        return $"MRN-{datePart}-{randomPart}";
    }
}
