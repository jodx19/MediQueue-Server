// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Infrastructure\InfrastructureServiceExtensions.cs
using Azure.Storage.Blobs;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using MediQueue.Application.Interfaces;
using MediQueue.Domain.Interfaces;
using MediQueue.Infrastructure.ExternalServices;
using MediQueue.Infrastructure.Persistence;
using MediQueue.Infrastructure.Persistence.Context;
using MediQueue.Infrastructure.Persistence.Repositories;
using MediQueue.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace MediQueue.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 0. Settings
        services.Configure<MediQueue.Infrastructure.Persistence.Settings.SeedingSettings>(
            configuration.GetSection("SeedingSettings"));

        // 1. DbContext
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found in configuration.");

        services.AddDbContext<ClinicDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
                sql.MigrationsAssembly(typeof(ClinicDbContext).Assembly.FullName)
                   .EnableRetryOnFailure(3, TimeSpan.FromSeconds(10), null)));

        // 2. Repositories & Unit of Work (scoped)
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IPatientRepository, PatientRepository>();
        services.AddScoped<IDoctorRepository, DoctorRepository>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<IClinicalVisitRepository, ClinicalVisitRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IDataSeeder, DataSeeder>();

        // 3. External Services
        services.AddScoped<IEmailService, EmailNotificationService>();
        services.AddScoped<ISmsService, ConsoleSmsService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenService, Services.TokenService>();
        services.AddScoped<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();
        services.AddScoped<IRealtimeService, SignalRRealtimeService>();
        services.AddScoped<ISchedulerService, HangfireSchedulerService>();
        services.AddScoped<IStorageService, AzureBlobStorageService>();
        services.AddScoped<ICacheService, RedisCacheService>();
        services.AddScoped<MissedAppointmentJob>();
        services.AddScoped<InvoiceOverdueJob>();
        services.AddScoped<DashboardJobs>();

        // 4. Auth & Context
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, MediQueue.Infrastructure.Services.CurrentUserService>();
        // 6. SignalR
        services.AddSignalR();

        // 5. Redis — StackExchange client (for prefix-scan) + distributed cache
        var redisConnStr = configuration["Redis:ConnectionString"] ?? "localhost:6379";

        services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            var options = ConfigurationOptions.Parse(redisConnStr);
            options.AbortOnConnectFail = false;
            return ConnectionMultiplexer.Connect(options);
        });

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnStr;
            options.InstanceName = "MediQueue:";
        });

        // 7. Azure Blob Storage — FIXED: register BlobServiceClient as singleton
        var blobConnStr = configuration["AzureBlob:ConnectionString"]
            ?? "UseDevelopmentStorage=true"; // fallback to Azurite for local dev

        services.AddSingleton(_ => new BlobServiceClient(blobConnStr));

        return services;
    }
}
