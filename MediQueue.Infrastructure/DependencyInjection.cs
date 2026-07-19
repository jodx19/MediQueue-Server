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
using StackExchange.Redis;
using Hangfire;

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
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();

        // 4. Core Services
        services.AddScoped<IUsageValidatorService, UsageValidatorService>();
        services.AddScoped<IEmailService, EmailNotificationService>();
        services.AddScoped<IAuthService, MediQueue.Infrastructure.ExternalServices.AuthService>();
        services.AddScoped<ITokenService, MediQueue.Infrastructure.Services.TokenService>();
        services.AddScoped<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();
        services.AddScoped<IStorageService, AzureBlobStorageService>();

        // 5. Caching (Redis in Production/Dev if connection string present, fallback to Memory)
        var redisConnectionString = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redisConnectionString))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
                options.InstanceName = "MediQueue:";
            });
            services.AddSingleton<IConnectionMultiplexer>(sp => 
                ConnectionMultiplexer.Connect(redisConnectionString));
            services.AddSingleton<ICacheService, RedisCacheService>();
        }
        else
        {
            services.AddSingleton<ICacheService, MemoryCacheService>();
        }

        // 6. Scheduler (Hangfire in Production if Connection String present, fallback to Dev Scheduler)
        var hangfireConnectionString = configuration.GetConnectionString("HangfireConnection")
            ?? configuration.GetConnectionString("DefaultConnection");

        var isProduction = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production";

        if (!string.IsNullOrWhiteSpace(hangfireConnectionString) && isProduction)
        {
            services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(hangfireConnectionString));
            services.AddHangfireServer();
            services.AddScoped<ISchedulerService, HangfireSchedulerService>();
        }
        else
        {
            services.AddScoped<ISchedulerService, Services.DevelopmentSchedulerService>();
        }

        // 7. SMS Service (Twilio if credentials present, fallback to Console)
        var twilioAccountSid = configuration["Twilio:AccountSid"];
        if (!string.IsNullOrWhiteSpace(twilioAccountSid) &&
            !twilioAccountSid.StartsWith("REPLACE_WITH"))
        {
            services.AddScoped<ISmsService, TwilioSmsService>();
        }
        else
        {
            services.AddScoped<ISmsService, ConsoleSmsService>();
        }

        // 8. WhatsApp Service (Twilio WhatsApp if credentials present, fallback to Console)
        var whatsAppNumber = configuration["Twilio:WhatsAppFromNumber"];
        if (!string.IsNullOrWhiteSpace(twilioAccountSid) &&
            !twilioAccountSid.StartsWith("REPLACE_WITH") &&
            !string.IsNullOrWhiteSpace(whatsAppNumber) &&
            !whatsAppNumber.StartsWith("REPLACE_WITH"))
        {
            services.AddScoped<IWhatsAppService, TwilioWhatsAppService>();
        }
        else
        {
            services.AddScoped<IWhatsAppService, ConsoleWhatsAppService>();
        }

        // 9. Groq AI Service
        var groqApiKey = configuration["Groq:ApiKey"];
        services.AddHttpClient("Groq", client =>
        {
            client.BaseAddress = new Uri(
                "https://api.groq.com/openai/v1/");
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        if (!string.IsNullOrWhiteSpace(groqApiKey) &&
            !groqApiKey.StartsWith("REPLACE_WITH"))
        {
            services.AddScoped<IGroqService, GroqLlmService>();
        }
        else
        {
            services.AddScoped<IGroqService, FallbackGroqService>();
        }
        
        // 10. Health Checks
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
