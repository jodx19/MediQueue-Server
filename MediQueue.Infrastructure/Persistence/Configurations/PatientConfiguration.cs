// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Infrastructure\Persistence\Configurations\PatientConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MediQueue.Domain.Entities;
using MediQueue.Domain.Enums;

namespace MediQueue.Infrastructure.Persistence.Configurations;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.NationalId).IsUnique();
        builder.Property(x => x.NationalId).HasMaxLength(14).IsRequired();

        builder.HasIndex(x => x.MedicalRecordNumber).IsUnique();
        builder.Property(x => x.MedicalRecordNumber).HasMaxLength(20).IsRequired();

        builder.Property(x => x.BloodType).HasConversion<string>();
        builder.Property(x => x.Gender).HasConversion<string>();
        builder.Property(x => x.MaritalStatus).HasConversion<string>();

        builder.OwnsOne(x => x.PersonName, n =>
        {
            n.Property(p => p.FirstName).HasMaxLength(50).IsRequired().HasColumnName("FirstName");
            n.Property(p => p.LastName).HasMaxLength(50).IsRequired().HasColumnName("LastName");
            n.Property(p => p.MiddleName).HasMaxLength(50).HasColumnName("MiddleName");
        });

        builder.OwnsOne(x => x.ContactInfo, c =>
        {
            c.Property(p => p.Phone).HasMaxLength(20).IsRequired().HasColumnName("Phone");
            c.Property(p => p.Email).HasMaxLength(100).HasColumnName("Email");
            c.Property(p => p.AlternativePhone).HasMaxLength(20).HasColumnName("AlternativePhone");
        });

        builder.OwnsOne(x => x.Address, a =>
        {
            a.Property(p => p.Street).HasMaxLength(200).IsRequired().HasColumnName("Street");
            a.Property(p => p.City).HasMaxLength(50).IsRequired().HasColumnName("City");
            a.Property(p => p.Governorate).HasMaxLength(50).IsRequired().HasColumnName("Governorate");
            a.Property(p => p.Country).HasMaxLength(50).HasColumnName("Country");
            a.Property(p => p.PostalCode).HasMaxLength(20).HasColumnName("PostalCode");
        });

        builder.OwnsMany(x => x.Allergies, a =>
        {
            a.ToTable("PatientAllergies");
            a.HasKey(al => al.Id);
            a.Property(al => al.Severity).HasConversion<string>();
            a.WithOwner().HasForeignKey("PatientId");
        });

        builder.OwnsMany(x => x.ChronicConditions, c =>
        {
            c.ToTable("PatientConditions");
            c.HasKey(cc => cc.Id);
            c.WithOwner().HasForeignKey("PatientId");
        });

        builder.OwnsMany(x => x.CurrentMedications, m =>
        {
            m.ToTable("PatientMedications");
            m.HasKey(cm => cm.Id);
            m.WithOwner().HasForeignKey("PatientId");
        });
    }
}
