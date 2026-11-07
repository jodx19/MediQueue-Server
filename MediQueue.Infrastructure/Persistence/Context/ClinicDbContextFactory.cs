using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using MediQueue.Application.Common;
using MediQueue.Application.Interfaces;

namespace MediQueue.Infrastructure.Persistence.Context;

public class ClinicDbContextFactory : IDesignTimeDbContextFactory<ClinicDbContext>
{
    private class DesignTimeCurrentUserService : ICurrentUserService
    {
        public Guid? UserId => null;
        public Guid TenantId => Guid.Empty;
        public string? Email => null;
        public string? Role => null;
        public Guid? PatientId => null;
        public Guid? DoctorId => null;
        public bool IsAuthenticated => false;
        public bool IsInRole(string role) => false;
    }

    private class DesignTimeTenantContext : ITenantContext
    {
        public Guid TenantId => Guid.Empty;
        public string Subdomain => string.Empty;
        public bool HasResolvedTenant => true;
    }

    public ClinicDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ClinicDbContext>();
        optionsBuilder.UseNpgsql(
            "Host=localhost;Database=mediqueue;Username=postgres;Password=postgres",
            npgsql => npgsql.MigrationsAssembly(typeof(ClinicDbContext).Assembly.FullName));

        return new ClinicDbContext(
            optionsBuilder.Options,
            null!,
            new DesignTimeCurrentUserService(),
            new DesignTimeTenantContext());
    }
}
