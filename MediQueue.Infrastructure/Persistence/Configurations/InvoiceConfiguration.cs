// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Infrastructure\Persistence\Configurations\InvoiceConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MediQueue.Domain.Entities;

namespace MediQueue.Infrastructure.Persistence.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.InvoiceNumber).IsUnique();
        builder.Property(x => x.InvoiceNumber).HasMaxLength(50).IsRequired();

        builder.Property(x => x.Status)
            .HasConversion(
                v => v.ToString(),
                v => (Domain.Enums.InvoiceStatus)System.Enum.Parse(typeof(Domain.Enums.InvoiceStatus), v));

        builder.HasOne(i => i.Appointment)
            .WithMany()
            .HasForeignKey(i => i.AppointmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(i => i.Patient)
            .WithMany()
            .HasForeignKey(i => i.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        // --- Stored Money value objects ---
        builder.OwnsOne(x => x.SubTotal, m =>
        {
            m.Property(p => p.Amount).HasColumnType("decimal(18,2)").HasColumnName("SubTotalAmount");
            m.Property(p => p.Currency).HasMaxLength(3).HasColumnName("SubTotalCurrency");
        });

        builder.OwnsOne(x => x.DiscountAmount, m =>
        {
            m.Property(p => p.Amount).HasColumnType("decimal(18,2)").HasColumnName("DiscountAmount");
            m.Property(p => p.Currency).HasMaxLength(3).HasColumnName("DiscountCurrency");
        });

        builder.OwnsOne(x => x.TaxAmount, m =>
        {
            m.Property(p => p.Amount).HasColumnType("decimal(18,2)").HasColumnName("TaxAmount");
            m.Property(p => p.Currency).HasMaxLength(3).HasColumnName("TaxCurrency");
        });

        builder.OwnsOne(x => x.PaidAmount, m =>
        {
            m.Property(p => p.Amount).HasColumnType("decimal(18,2)").HasColumnName("PaidAmount");
            m.Property(p => p.Currency).HasMaxLength(3).HasColumnName("PaidCurrency");
        });

        // NOTE: TotalAmount and RemainingAmount are COMPUTED expression-body properties.
        // They are intentionally NOT mapped — EF Core cannot persist expression members.
        // TotalAmount  = SubTotal - DiscountAmount + TaxAmount  (computed at runtime)
        // RemainingAmount = TotalAmount - PaidAmount              (computed at runtime)

        builder.OwnsMany(x => x.Items, i =>
        {
            i.ToTable("InvoiceItems");
            i.HasKey(ii => ii.Id);

            i.OwnsOne(ii => ii.UnitPrice, m =>
            {
                m.Property(p => p.Amount).HasColumnType("decimal(18,2)").HasColumnName("UnitPriceAmount");
                m.Property(p => p.Currency).HasMaxLength(3).HasColumnName("UnitPriceCurrency");
            });


            i.WithOwner().HasForeignKey("InvoiceId");
        });

        builder.OwnsMany(x => x.Payments, p =>
        {
            p.ToTable("InvoicePayments");
            p.HasKey(ip => ip.Id);

            p.Property(ip => ip.Method)
                .HasConversion(
                    v => v.ToString(),
                    v => (Domain.Enums.PaymentMethod)System.Enum.Parse(typeof(Domain.Enums.PaymentMethod), v));

            p.OwnsOne(ip => ip.Amount, m =>
            {
                m.Property(c => c.Amount).HasColumnType("decimal(18,2)").HasColumnName("Amount");
                m.Property(c => c.Currency).HasMaxLength(3).HasColumnName("Currency");
            });

            p.WithOwner().HasForeignKey("InvoiceId");
        });
    }
}
