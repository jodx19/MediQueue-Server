// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Infrastructure\Persistence\Context\ClinicDbContext.cs
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MediQueue.Domain.Common;
using MediQueue.Domain.Entities;
using MediQueue.Infrastructure.Persistence.Configurations;
using MediQueue.Application.Common;
using MediQueue.Application.Interfaces;
using MediQueue.Infrastructure.Persistence.Entities;

namespace MediQueue.Infrastructure.Persistence.Context;

public class ClinicDbContext : DbContext
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ITenantContext _tenantContext;

    public ClinicDbContext(
        DbContextOptions<ClinicDbContext> options, 
        IMediator mediator,
        ICurrentUserService currentUserService,
        ITenantContext tenantContext) : base(options)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _tenantContext = tenantContext;
    }

    public Guid CurrentTenantId => _currentUserService.TenantId;

    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<ClinicalVisit> ClinicalVisits => Set<ClinicalVisit>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<MedicalAttachment> Attachments => Set<MedicalAttachment>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<ClinicSettings> ClinicSettings => Set<ClinicSettings>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ClinicDbContext).Assembly);

        modelBuilder.Entity<ClinicSettings>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ClinicName).HasMaxLength(200);
            entity.Property(e => e.Currency).HasMaxLength(3);
            entity.Property(e => e.DepositAmount)
                  .HasColumnType("decimal(18,2)");
        });

        // Global Query Filter for Soft Deletes & Multi-Tenancy
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.IsOwned()) continue;

            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");

                // IsDeleted == false
                var isDeletedProperty = System.Linq.Expressions.Expression.Property(parameter, "IsDeleted");
                var isNotDeleted = System.Linq.Expressions.Expression.Equal(
                    isDeletedProperty,
                    System.Linq.Expressions.Expression.Constant(false));

                // TenantId == CurrentTenantId
                var tenantIdProperty = System.Linq.Expressions.Expression.Property(parameter, "TenantId");

                // Public properties on DbContext — safe for EF design-time expression trees
                var dbContextExpression  = System.Linq.Expressions.Expression.Constant(this);
                var currentTenantIdProp  = System.Linq.Expressions.Expression.Property(dbContextExpression, "CurrentTenantId");

                var isCurrentTenant = System.Linq.Expressions.Expression.Equal(
                    tenantIdProperty,
                    currentTenantIdProp);

                // TenantId == current AND IsDeleted == false
                var combinedExpression = System.Linq.Expressions.Expression.AndAlso(
                    isCurrentTenant,
                    isNotDeleted);

                var lambda = System.Linq.Expressions.Expression.Lambda(combinedExpression, parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }

        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.HasIndex(t => t.Subdomain).IsUnique();
            entity.Property(t => t.Name).HasMaxLength(200);
            entity.Property(t => t.Subdomain).HasMaxLength(100);
            entity.Property(t => t.AdminEmail).HasMaxLength(256);
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var baseEntries = ChangeTracker.Entries<BaseEntity>();
        var tenantId = _currentUserService.TenantId;

        foreach (var entry in baseEntries)
        {
            if (entry.State == EntityState.Added)
            {
                // Auto-stamp TenantId if not set and context has one
                if (entry.Entity.TenantId == Guid.Empty && tenantId != Guid.Empty)
                {
                    entry.Entity.TenantId = tenantId;
                }
            }
        }

        var auditableEntries = ChangeTracker.Entries<AuditableEntity>();
        var now = DateTime.UtcNow;

        foreach (var entry in auditableEntries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property(a => a.CreatedAt).CurrentValue = now;
                entry.Property(a => a.CreatedBy).CurrentValue = _currentUserService.UserId?.ToString() ?? "System";
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(a => a.UpdatedAt).CurrentValue = now;
                entry.Property(a => a.UpdatedBy).CurrentValue = _currentUserService.UserId?.ToString() ?? "System";
            }
        }

        // Get domain events before save, dispatch after save
        var entitiesWithEvents = ChangeTracker.Entries<BaseAggregateRoot>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Any())
            .ToList();

        var domainEvents = entitiesWithEvents
            .SelectMany(e => e.DomainEvents)
            .ToList();

        var result = await base.SaveChangesAsync(cancellationToken);

        // Dispatch events
        foreach (var domainEvent in domainEvents)
        {
            var notificationType = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());
            var notification = Activator.CreateInstance(notificationType, domainEvent);
            if (notification != null)
            {
                await _mediator.Publish(notification, cancellationToken);
            }
        }
        
        entitiesWithEvents.ForEach(e => e.ClearDomainEvents());

        return result;
    }
}
