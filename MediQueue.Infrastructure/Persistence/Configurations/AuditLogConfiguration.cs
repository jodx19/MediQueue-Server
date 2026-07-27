using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MediQueue.Domain.Entities;

namespace MediQueue.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Action).HasMaxLength(200).IsRequired();
        builder.Property(a => a.UserEmail).HasMaxLength(256);
        builder.Property(a => a.UserRole).HasMaxLength(50);
        builder.Property(a => a.ErrorMessage).HasMaxLength(1000);

        // Indexes for efficient querying
        builder.HasIndex(a => a.TenantId);
        builder.HasIndex(a => a.UserId);
        builder.HasIndex(a => a.Timestamp);
    }
}
