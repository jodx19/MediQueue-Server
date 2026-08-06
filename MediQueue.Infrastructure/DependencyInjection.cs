using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MediQueue.Application.Interfaces;
using MediQueue.Domain.Interfaces;
using MediQueue.Infrastructure.ExternalServices;
using MediQueue.Infrastructure.Persistence;
using MediQueue.Infrastructure.Persistence.Context;
using MediQueue.Infrastructure.Persistence.Repositories;
using MediQueue.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Builder;
using MediQueue.Infrastructure.Services;
using MediQueue.Infrastructure.Repositories;

namespace MediQueue.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 1. Settings
        services.Configure<MediQueue.Infrastructure.Persistence.Settings.SeedingSettings>(
            configuration.GetSection("SeedingSettings"));

        // 2. DbContext
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ClinicDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
                sql.MigrationsAssembly(typeof(ClinicDbContext).Assembly.FullName)));

        // 3. Repositories & Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IPatientRepository, PatientRepository>();
        services.AddScoped<IDoctorRepository, DoctorRepository>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<IClinicalVisitRepository, ClinicalVisitRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<ISettingsRepository, SettingsRepository>();
        services.AddScoped<ITenantRepository, TenantRepository>();

        // 4. Core Services
        services.AddScoped<IEmailService, EmailNotificationService>();
        services.AddScoped<ISmsService, ConsoleSmsService>();
        services.AddScoped<IAuthService, MediQueue.Infrastructure.ExternalServices.AuthService>();
        services.AddScoped<ITokenService, MediQueue.Infrastructure.Services.TokenService>();
        services.AddScoped<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();
        services.AddScoped<IStorageService, AzureBlobStorageService>();
        services.AddScoped<IAppSettingsService, MediQueue.Infrastructure.Services.AppSettingsService>();
        
        // 5. Simplified Services for Development (No Redis, No Hangfire)
        services.AddSingleton<ICacheService, MemoryCacheService>();
        services.AddScoped<ISchedulerService, Services.DevelopmentSchedulerService>();
        
        // 6. Health Checks
        services.AddHealthChecks()
            .AddSqlServer(connectionString, name: "sql-server", tags: ["db", "infrastructure"]);

        return services;
    }

    public static async Task UseInfrastructure(this IApplicationBuilder app)
    {
        // For development: ensure DB is created and migrated
        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ClinicDbContext>();
        
        // await context.Database.MigrateAsync(); // Uncomment if you want auto-migrations on start
        await Task.CompletedTask;
    }
}
