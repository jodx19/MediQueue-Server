// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Infrastructure\Persistence\Configurations\AppointmentConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MediQueue.Domain.Entities;

namespace MediQueue.Infrastructure.Persistence.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.DoctorId, x.ScheduledAt });

        builder.Property(x => x.RowVersion).IsRowVersion().IsConcurrencyToken();

        builder.Property(x => x.Status)
            .HasConversion(v => v.ToString(), v => (Domain.Enums.AppointmentStatus)System.Enum.Parse(typeof(Domain.Enums.AppointmentStatus), v));

        builder.Property(x => x.Priority)
            .HasConversion(v => v.ToString(), v => (Domain.Enums.AppointmentPriority)System.Enum.Parse(typeof(Domain.Enums.AppointmentPriority), v));

        builder.Property(x => x.VisitType)
            .HasConversion(v => v.ToString(), v => (Domain.Enums.VisitType)System.Enum.Parse(typeof(Domain.Enums.VisitType), v));

        builder.HasOne<Patient>()
            .WithMany()
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Doctor>()
            .WithMany()
            .HasForeignKey(x => x.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
