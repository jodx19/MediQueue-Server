// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Infrastructure\Persistence\Configurations\ClinicalVisitConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MediQueue.Domain.Entities;
using MediQueue.Domain.Enums;
using MediQueue.Domain.ValueObjects;

namespace MediQueue.Infrastructure.Persistence.Configurations;

public class ClinicalVisitConfiguration : IEntityTypeConfiguration<ClinicalVisit>
{
    public void Configure(EntityTypeBuilder<ClinicalVisit> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.IsFinalized).IsRequired().HasDefaultValue(false);

        builder.HasOne<Patient>()
            .WithMany()
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Doctor>()
            .WithMany()
            .HasForeignKey(x => x.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Appointment>()
            .WithMany()
            .HasForeignKey(x => x.AppointmentId)
            .OnDelete(DeleteBehavior.Restrict);

        // VitalSigns stored as a JSON column
        builder.OwnsMany(x => x.VitalSigns, vs =>
        {
            vs.ToJson();
            vs.Property(v => v.Type).HasConversion<string>();
        });

        builder.OwnsMany(x => x.Diagnoses, d =>
        {
            d.ToTable("VisitDiagnoses");
            d.HasKey(vd => vd.Id);
            d.Property(vd => vd.Type).HasConversion<string>();

            d.OwnsOne(vd => vd.MedicalCode, mc =>
            {
                mc.Property(p => p.System).HasMaxLength(50).HasColumnName("CodeSystem").HasConversion<string>();
                mc.Property(p => p.Code).HasMaxLength(50).HasColumnName("CodeValue");
                mc.Property(p => p.Description).HasMaxLength(500).HasColumnName("CodeDescription");
            });
            d.WithOwner().HasForeignKey("VisitId");
        });

        builder.OwnsMany(x => x.Procedures, p =>
        {
            p.ToTable("VisitProcedures");
            p.HasKey(vp => vp.Id);

            p.OwnsOne(vp => vp.MedicalCode, mc =>
            {
                mc.Property(c => c.System).HasMaxLength(50).HasColumnName("CodeSystem").HasConversion<string>();
                mc.Property(c => c.Code).HasMaxLength(50).HasColumnName("CodeValue");
                mc.Property(c => c.Description).HasMaxLength(500).HasColumnName("CodeDescription");
            });

            p.OwnsOne(vp => vp.Fee, f =>
            {
                f.Property(c => c.Amount).HasColumnType("decimal(18,2)").HasColumnName("FeeAmount");
                f.Property(c => c.Currency).HasMaxLength(3).HasColumnName("FeeCurrency");
            });
            p.WithOwner().HasForeignKey("VisitId");
        });

        builder.OwnsMany(x => x.LabRequests, lr =>
        {
            lr.ToTable("VisitLabRequests");
            lr.HasKey(vlr => vlr.Id);
            lr.Property(vlr => vlr.Status).HasConversion<string>();
            lr.WithOwner().HasForeignKey("VisitId");
        });

        builder.OwnsMany(x => x.ImagingRequests, ir =>
        {
            ir.ToTable("VisitImagingRequests");
            ir.HasKey(vir => vir.Id);
            ir.Property(vir => vir.ImagingType).HasConversion<string>();
            ir.Property(vir => vir.Status).HasConversion<string>();
            ir.WithOwner().HasForeignKey("VisitId");
        });

        builder.OwnsMany(x => x.Referrals, r =>
        {
            r.ToTable("VisitReferrals");
            r.HasKey(vr => vr.Id);
            r.Property(vr => vr.ReferredToSpecialty).HasConversion<string>();
            r.Property(vr => vr.Urgency).HasConversion<string>();
            r.WithOwner().HasForeignKey("VisitId");
        });

        builder.OwnsMany(x => x.Prescriptions, p =>
        {
            p.ToTable("VisitPrescriptions");
            p.HasKey(vp => vp.Id);
            p.WithOwner().HasForeignKey("VisitId");

            p.OwnsMany(vp => vp.Items, pi =>
            {
                pi.ToTable("PrescriptionItems");
                pi.WithOwner().HasForeignKey("PrescriptionId");
            });
        });
    }
}
