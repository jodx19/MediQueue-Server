using MediQueue.Domain.Common;

namespace MediQueue.Domain.Entities;

/// <summary>
/// Medical history entity for comprehensive patient health tracking
/// Follows healthcare data standards and HIPAA compliance
/// </summary>
public class MedicalHistory : AuditableEntity
{
    /// <summary>
    /// Foreign key to the patient
    /// </summary>
    /// <summary>
    /// Foreign key to the patient
    /// </summary>
    public Guid PatientId { get; private set; }
    
    /// <summary>
    /// Navigation property to the patient
    /// </summary>
    public Patient? Patient { get; private set; }
    
    // VITAL SIGNS & PHYSICAL MEASUREMENTS
    /// <summary>
    /// Height in centimeters
    /// </summary>
    public decimal? Height { get; private set; }
    
    /// <summary>
    /// Weight in kilograms
    /// </summary>
    public decimal? Weight { get; private set; }
    
    /// <summary>
    /// Calculated BMI
    /// </summary>
    public decimal? Bmi { get; private set; }
    
    /// <summary>
    /// Blood pressure reading (systolic/diastolic)
    /// </summary>
    public string? BloodPressure { get; private set; }
    
    /// <summary>
    /// Heart rate in beats per minute
    /// </summary>
    public int? HeartRate { get; private set; }
    
    /// <summary>
    /// Respiratory rate in breaths per minute
    /// </summary>
    public int? RespiratoryRate { get; private set; }
    
    /// <summary>
    /// Body temperature in Celsius
    /// </summary>
    public decimal? Temperature { get; private set; }
    
    /// <summary>
    /// Blood oxygen saturation percentage
    /// </summary>
    public int? OxygenSaturation { get; private set; }
    
    // LIFESTYLE & SOCIAL HISTORY
    /// <summary>
    /// Smoking status
    /// </summary>
    public bool IsSmoker { get; private set; }
    
    /// <summary>
    /// Number of smoking pack years
    /// </summary>
    public int? SmokingPackYears { get; private set; }
    
    /// <summary>
    /// Alcohol consumption status
    /// </summary>
    public bool IsAlcoholConsumer { get; private set; }
    
    /// <summary>
    /// Details about alcohol consumption
    /// </summary>
    public string? AlcoholConsumptionDetails { get; private set; }
    
    /// <summary>
    /// Drug use status
    /// </summary>
    public bool IsDrugUser { get; private set; }
    
    /// <summary>
    /// Details about drug use
    /// </summary>
    public string? DrugUseDetails { get; private set; }
    
    /// <summary>
    /// Exercise frequency
    /// </summary>
    public string? ExerciseFrequency { get; private set; }
    
    /// <summary>
    /// Diet type
    /// </summary>
    public string? DietType { get; private set; }
    
    // ALLERGIES
    /// <summary>
    /// Has known allergies
    /// </summary>
    public bool HasAllergies { get; private set; }
    
    /// <summary>
    /// Detailed allergy information
    /// </summary>
    public string? AllergyDetails { get; private set; }
    
    /// <summary>
    /// Has medication allergies
    /// </summary>
    public bool HasMedicationAllergies { get; private set; }
    
    /// <summary>
    /// Detailed medication allergy information
    /// </summary>
    public string? MedicationAllergyDetails { get; private set; }
    
    // CHRONIC CONDITIONS
    /// <summary>
    /// Has chronic conditions
    /// </summary>
    public bool HasChronicConditions { get; private set; }
    
    /// <summary>
    /// Detailed chronic condition information
    /// </summary>
    public string? ChronicConditionDetails { get; private set; }
    
    /// <summary>
    /// Has diabetes
    /// </summary>
    public bool HasDiabetes { get; private set; }
    
    /// <summary>
    /// Type of diabetes (Type 1, Type 2, Gestational)
    /// </summary>
    public string? DiabetesType { get; private set; }
    
    /// <summary>
    /// Has hypertension
    /// </summary>
    public bool HasHypertension { get; private set; }
    
    /// <summary>
    /// Has heart disease
    /// </summary>
    public bool HasHeartDisease { get; private set; }
    
    /// <summary>
    /// Has asthma
    /// </summary>
    public bool HasAsthma { get; private set; }
    
    // FAMILY HISTORY
    /// <summary>
    /// Family history of chronic diseases
    /// </summary>
    public string? FamilyHistory { get; private set; }
    
    // SURGICAL HISTORY
    /// <summary>
    /// Previous surgical procedures
    /// </summary>
    public string? SurgicalHistory { get; private set; }
    
    // MEDICATION HISTORY
    /// <summary>
    /// Current medications
    /// </summary>
    public string? CurrentMedications { get; private set; }
    
    /// <summary>
    /// Past medications
    /// </summary>
    public string? PastMedications { get; private set; }
    
    // IMMUNIZATION HISTORY
    /// <summary>
    /// Immunization records
    /// </summary>
    public string? ImmunizationHistory { get; private set; }
    
    // WOMEN'S HEALTH (if applicable)
    /// <summary>
    /// Last menstrual period
    /// </summary>
    public DateTime? LastMenstrualPeriod { get; private set; }
    
    /// <summary>
    /// Pregnancy status
    /// </summary>
    public bool IsPregnant { get; private set; }
    
    /// <summary>
    /// Number of pregnancies
    /// </summary>
    public int? PregnancyCount { get; private set; }
    
    // ADDITIONAL NOTES
    /// <summary>
    /// Additional medical notes
    /// </summary>
    public string? AdditionalNotes { get; private set; }
    
    /// <summary>
    /// Date of last medical examination
    /// </summary>
    public DateTime? LastExaminationDate { get; private set; }
    
    /// <summary>
    /// Name of examining physician
    /// </summary>
    public string? ExaminingPhysician { get; private set; }
    
    // NAVIGATION PROPERTIES
    // public ICollection<MedicalRecord>? MedicalRecords { get; set; }
    
    // BUSINESS METHODS
    
    /// <summary>
    /// Updates vital signs measurements
    /// </summary>
    public void UpdateVitalSigns(
        decimal? height = null,
        decimal? weight = null,
        string? bloodPressure = null,
        int? heartRate = null,
        int? respiratoryRate = null,
        decimal? temperature = null,
        int? oxygenSaturation = null,
        string? updatedBy = null)
    {
        Height = height;
        Weight = weight;
        BloodPressure = bloodPressure?.Trim();
        HeartRate = heartRate;
        RespiratoryRate = respiratoryRate;
        Temperature = temperature;
        OxygenSaturation = oxygenSaturation;
        
        // Calculate BMI if height and weight are available
        if (height.HasValue && weight.HasValue && height > 0)
        {
            Bmi = weight.Value / ((height.Value / 100) * (height.Value / 100));
        }
        
        SetUpdated();
        if (updatedBy != null) SetUpdatedBy(updatedBy);
    }
    
    /// <summary>
    /// Updates lifestyle information
    /// </summary>
    public void UpdateLifestyle(
        bool isSmoker,
        int? smokingPackYears = null,
        bool isAlcoholConsumer = false,
        string? alcoholDetails = null,
        bool isDrugUser = false,
        string? drugDetails = null,
        string? exerciseFrequency = null,
        string? dietType = null,
        string? updatedBy = null)
    {
        IsSmoker = isSmoker;
        SmokingPackYears = isSmoker ? smokingPackYears : null;
        IsAlcoholConsumer = isAlcoholConsumer;
        AlcoholConsumptionDetails = isAlcoholConsumer ? alcoholDetails?.Trim() : null;
        IsDrugUser = isDrugUser;
        DrugUseDetails = isDrugUser ? drugDetails?.Trim() : null;
        ExerciseFrequency = exerciseFrequency?.Trim();
        DietType = dietType?.Trim();
        
        SetUpdated();
        if (updatedBy != null) SetUpdatedBy(updatedBy);
    }
    
    /// <summary>
    /// Updates allergy information
    /// </summary>
    public void UpdateAllergies(
        bool hasAllergies,
        string? allergyDetails = null,
        bool hasMedicationAllergies = false,
        string? medicationAllergyDetails = null,
        string? updatedBy = null)
    {
        HasAllergies = hasAllergies;
        AllergyDetails = hasAllergies ? allergyDetails?.Trim() : null;
        HasMedicationAllergies = hasMedicationAllergies;
        MedicationAllergyDetails = hasMedicationAllergies ? medicationAllergyDetails?.Trim() : null;
        
        SetUpdated();
        if (updatedBy != null) SetUpdatedBy(updatedBy);
    }
    
    /// <summary>
    /// Updates chronic conditions
    /// </summary>
    public void UpdateChronicConditions(
        bool hasChronicConditions,
        string? chronicConditionDetails = null,
        bool hasDiabetes = false,
        string? diabetesType = null,
        bool hasHypertension = false,
        bool hasHeartDisease = false,
        bool hasAsthma = false,
        string? updatedBy = null)
    {
        HasChronicConditions = hasChronicConditions;
        ChronicConditionDetails = hasChronicConditions ? chronicConditionDetails?.Trim() : null;
        HasDiabetes = hasDiabetes;
        DiabetesType = hasDiabetes ? diabetesType?.Trim() : null;
        HasHypertension = hasHypertension;
        HasHeartDisease = hasHeartDisease;
        HasAsthma = hasAsthma;
        
        SetUpdated();
        if (updatedBy != null) SetUpdatedBy(updatedBy);
    }
    
    /// <summary>
    /// Updates women's health information
    /// </summary>
    public void UpdateWomensHealth(
        DateTime? lastMenstrualPeriod = null,
        bool isPregnant = false,
        int? pregnancyCount = null,
        string? updatedBy = null)
    {
        LastMenstrualPeriod = lastMenstrualPeriod;
        IsPregnant = isPregnant;
        PregnancyCount = isPregnant ? pregnancyCount : null;
        
        SetUpdated();
        if (updatedBy != null) SetUpdatedBy(updatedBy);
    }
    
    /// <summary>
    /// Updates examination information
    /// </summary>
    public void UpdateExamination(
        DateTime examinationDate,
        string physicianName,
        string? updatedBy = null)
    {
        LastExaminationDate = examinationDate;
        ExaminingPhysician = physicianName?.Trim();
        
        SetUpdated();
        if (updatedBy != null) SetUpdatedBy(updatedBy);
    }
    
    /// <summary>
    /// Gets BMI category
    /// </summary>
    public string? GetBmiCategory()
    {
        if (!Bmi.HasValue) return null;
        
        return Bmi.Value switch
        {
            < 18.5m => "Underweight",
            < 25m => "Normal",
            < 30m => "Overweight",
            _ => "Obese"
        };
    }
    
    /// <summary>
    /// Gets blood pressure category
    /// </summary>
    public string? GetBloodPressureCategory()
    {
        if (string.IsNullOrWhiteSpace(BloodPressure)) return null;
        
        var parts = BloodPressure.Split('/');
        if (parts.Length != 2 || 
            !int.TryParse(parts[0], out var systolic) || 
            !int.TryParse(parts[1], out var diastolic))
            return null;
        
        return (systolic, diastolic) switch
        {
            (< 90, < 60) => "Low",
            (< 120, < 80) => "Normal",
            (< 130, < 80) => "Elevated",
            (< 140, < 90) => "High Stage 1",
            (< 180, < 120) => "High Stage 2",
            _ => "Hypertensive Crisis"
        };
    }
}
