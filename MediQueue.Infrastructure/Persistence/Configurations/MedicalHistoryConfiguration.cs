// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Infrastructure\Persistence\Configurations\MedicalHistoryConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MediQueue.Domain.Entities;

namespace MediQueue.Infrastructure.Persistence.Configurations;

public class MedicalHistoryConfiguration : IEntityTypeConfiguration<MedicalHistory>
{
    public void Configure(EntityTypeBuilder<MedicalHistory> builder)
    {
        builder.ToTable("MedicalHistories");

        builder.HasKey(mh => mh.Id);

        builder.Property(mh => mh.PatientId).IsRequired();

        builder.Property(mh => mh.Height).HasPrecision(5, 2);
        builder.Property(mh => mh.Weight).HasPrecision(5, 2);
        builder.Property(mh => mh.Bmi).HasPrecision(5, 2);
        builder.Property(mh => mh.BloodPressure).HasMaxLength(20);
        builder.Property(mh => mh.Temperature).HasPrecision(3, 1);

        builder.Property(mh => mh.AlcoholConsumptionDetails).HasMaxLength(500);
        builder.Property(mh => mh.DrugUseDetails).HasMaxLength(500);
        builder.Property(mh => mh.ExerciseFrequency).HasMaxLength(100);
        builder.Property(mh => mh.DietType).HasMaxLength(100);

        builder.Property(mh => mh.AllergyDetails).HasMaxLength(1000);
        builder.Property(mh => mh.MedicationAllergyDetails).HasMaxLength(1000);
        builder.Property(mh => mh.ChronicConditionDetails).HasMaxLength(1000);
        builder.Property(mh => mh.DiabetesType).HasMaxLength(50);

        builder.Property(mh => mh.FamilyHistory).HasMaxLength(2000);
        builder.Property(mh => mh.SurgicalHistory).HasMaxLength(2000);
        builder.Property(mh => mh.CurrentMedications).HasMaxLength(2000);
        builder.Property(mh => mh.PastMedications).HasMaxLength(2000);
        builder.Property(mh => mh.ImmunizationHistory).HasMaxLength(2000);
        builder.Property(mh => mh.AdditionalNotes).HasMaxLength(2000);

        builder.Property(mh => mh.ExaminingPhysician).HasMaxLength(200);

        builder.Property(mh => mh.CreatedBy).HasMaxLength(450);
        builder.Property(mh => mh.UpdatedBy).HasMaxLength(450);

        // Indexes
        builder.HasIndex(mh => mh.PatientId).HasDatabaseName("IX_MedicalHistories_PatientId");
        builder.HasIndex(mh => new { mh.PatientId, mh.LastExaminationDate }).HasDatabaseName("IX_MedicalHistories_Patient_ExamDate");

        // Relationships
        builder.HasOne(mh => mh.Patient)
            .WithMany() // Patient does not have a MedicalHistory collection or reference in this version
            .HasForeignKey(mh => mh.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        // Query filters
        builder.HasQueryFilter(mh => !mh.IsDeleted);
    }
}
