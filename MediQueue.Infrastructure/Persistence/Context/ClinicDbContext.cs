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

namespace MediQueue.Infrastructure.Persistence.Context;

public class ClinicDbContext : DbContext
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public ClinicDbContext(
        DbContextOptions<ClinicDbContext> options, 
        IMediator mediator,
        ICurrentUserService currentUserService) : base(options)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<ClinicalVisit> ClinicalVisits => Set<ClinicalVisit>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<MedicalAttachment> Attachments => Set<MedicalAttachment>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ClinicDbContext).Assembly);

        // Global Query Filter for Soft Deletes
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.IsOwned()) continue;

            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                var propertyMethodInfo = typeof(EF).GetMethod("Property")!.MakeGenericMethod(typeof(bool));
                var isDeletedProperty = System.Linq.Expressions.Expression.Call(propertyMethodInfo, parameter, System.Linq.Expressions.Expression.Constant("IsDeleted"));
                var compareExpression = System.Linq.Expressions.Expression.MakeBinary(System.Linq.Expressions.ExpressionType.Equal, isDeletedProperty, System.Linq.Expressions.Expression.Constant(false));
                var lambda = System.Linq.Expressions.Expression.Lambda(compareExpression, parameter);
                
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
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
