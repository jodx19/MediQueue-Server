// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Infrastructure\Persistence\Configurations\DoctorConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MediQueue.Domain.Entities;

namespace MediQueue.Infrastructure.Persistence.Configurations;

public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.LicenseNumber).IsUnique();
        builder.Property(x => x.LicenseNumber).HasMaxLength(50).IsRequired();

        builder.Property(x => x.Specialty)
            .HasConversion(v => v.ToString(), v => (Domain.Enums.MedicalSpecialty)System.Enum.Parse(typeof(Domain.Enums.MedicalSpecialty), v));

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

        builder.OwnsOne(x => x.ConsultationFee, f =>
        {
            f.Property(p => p.Amount).HasColumnType("decimal(18,2)").HasColumnName("ConsultationFeeAmount");
            f.Property(p => p.Currency).HasMaxLength(3).HasColumnName("ConsultationFeeCurrency");
        });

        builder.OwnsOne(x => x.FollowUpFee, f =>
        {
            f.Property(p => p.Amount).HasColumnType("decimal(18,2)").HasColumnName("FollowUpFeeAmount");
            f.Property(p => p.Currency).HasMaxLength(3).HasColumnName("FollowUpFeeCurrency");
        });

        builder.OwnsMany(x => x.Qualifications, q =>
        {
            q.ToTable("DoctorQualifications");
            q.HasKey(dq => dq.Id);
            q.WithOwner().HasForeignKey("DoctorId");
        });

        // JSON column for working shifts
        builder.OwnsMany(x => x.WorkingShifts, s =>
        {
            s.ToJson();
            s.Property(ws => ws.DayOfWeek).HasConversion(v => v.ToString(), v => (System.DayOfWeek)System.Enum.Parse(typeof(System.DayOfWeek), v));
        });
    }
}
