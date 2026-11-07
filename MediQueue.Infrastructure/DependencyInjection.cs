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
using Hangfire;
using Hangfire.SqlServer;
using Hangfire.PostgreSql;
using StackExchange.Redis;
using Microsoft.Extensions.Logging;

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

        var isPostgres = connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
            || connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase)
            || connectionString.Contains("Host=", StringComparison.OrdinalIgnoreCase);

        // Convert URI format (postgresql://user:pass@host/db?sslmode=require)
        // to Npgsql keyword format for maximum IIS compatibility
        string npgsqlConnectionString = connectionString;
        if (isPostgres && (connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
                        || connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase)))
        {
            try
            {
                var uri = new Uri(connectionString.Replace("postgresql://", "https://").Replace("postgres://", "https://"));
                var userInfo = uri.UserInfo.Split(':');
                var user = Uri.UnescapeDataString(userInfo[0]);
                var pass = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
                var host = uri.Host;
                var port = uri.Port > 0 ? uri.Port : 5432;
                var database = uri.AbsolutePath.TrimStart('/');
                var query = uri.Query.TrimStart('?');
                var sslMode = "Require";
                if (query.Contains("sslmode=disable", StringComparison.OrdinalIgnoreCase)) sslMode = "Disable";
                else if (query.Contains("sslmode=prefer", StringComparison.OrdinalIgnoreCase)) sslMode = "Prefer";

                var poolerConfig = host.Contains("-pooler", StringComparison.OrdinalIgnoreCase)
                    ? "Maximum Pool Size=10;"
                    : "";

                npgsqlConnectionString = $"Host={host};Port={port};Database={database};Username={user};Password={pass};SSL Mode={sslMode};Trust Server Certificate=true;Timeout=30;Command Timeout=60;{poolerConfig}";
            }
            catch
            {
                // If URI parsing fails, use the original connection string
                npgsqlConnectionString = connectionString;
            }
        }

        services.AddDbContext<ClinicDbContext>(options =>
        {
            if (isPostgres)
            {
                options.UseNpgsql(npgsqlConnectionString, npgsql =>
                {
                    npgsql.MigrationsAssembly(typeof(ClinicDbContext).Assembly.FullName);
                    npgsql.EnableRetryOnFailure(3);
                });
            }
            else
            {
                options.UseSqlServer(connectionString, sql =>
                    sql.MigrationsAssembly(typeof(ClinicDbContext).Assembly.FullName));
            }
        });

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
        services.AddScoped<IAuthService, MediQueue.Infrastructure.ExternalServices.AuthService>();
        services.AddScoped<ITokenService, MediQueue.Infrastructure.Services.TokenService>();
        services.AddScoped<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();
        // Azure Storage — only register if connection string is configured
        var azureConnStr = configuration["AzureStorage:ConnectionString"];
        if (!string.IsNullOrWhiteSpace(azureConnStr) && !azureConnStr.Contains("REPLACE_WITH"))
        {
            services.AddScoped<IStorageService, AzureBlobStorageService>();
        }
        else
        {
            services.AddScoped<IStorageService, StubStorageService>();
        }

        // 4a. SMS — Unifonic (Egyptian/Arab market)
        //     Configure via env vars: Sms__AppSid, Sms__SenderName
        //     Falls back to console logging if AppSid is not set (dev mode).
        services.Configure<UnifonicSmsOptions>(configuration.GetSection(UnifonicSmsOptions.SectionName));
        services.AddHttpClient<ISmsService, UnifonicSmsService>();

        // 4b. Drug Interaction — GPT-4o-mini
        //     Configure via env vars: OpenAI__ApiKey, OpenAI__Model
        services.Configure<GptDrugInteractionOptions>(configuration.GetSection(GptDrugInteractionOptions.SectionName));
        services.AddHttpClient<IDrugInteractionService, GptDrugInteractionService>();

        // 4c. Payment Gateway — currently Stub (no real provider integrated yet)
        //     To switch: replace StubPaymentService with PaymobPaymentService / FawryPaymentService
        //     and configure the provider credentials via env vars.
        services.AddSingleton<IPaymentGatewayService, StubPaymentService>();

        // 5. Caching — Redis if connection string is present, MemoryCache otherwise
        var redisConnStr = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redisConnStr) && !redisConnStr.Contains("localhost"))
        {
            try
            {
                var redisOptions = ConfigurationOptions.Parse(redisConnStr);
                redisOptions.AbortOnConnectFail = false;
                redisOptions.ConnectTimeout = 3000;
                redisOptions.SyncTimeout = 3000;

                var connectionMultiplexer = ConnectionMultiplexer.Connect(redisOptions);
                services.AddSingleton<IConnectionMultiplexer>(connectionMultiplexer);
                services.AddSingleton<ICacheService, RedisCacheService>();
                Console.WriteLine("--> Redis connection configured (background connect enabled).");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[Warning] Failed to configure Redis: {ex.Message}. Falling back to MemoryCache.");
                Console.ResetColor();

                services.AddMemoryCache();
                services.AddSingleton<ICacheService, MemoryCacheService>();
            }
        }
        else
        {
            services.AddMemoryCache();
            services.AddSingleton<ICacheService, MemoryCacheService>();
        }

        // 6. Hangfire — background jobs with PostgreSQL or SQL Server storage
        services.AddHangfire(config =>
        {
            config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                  .UseSimpleAssemblyNameTypeSerializer()
                  .UseRecommendedSerializerSettings();

            if (isPostgres)
            {
                config.UsePostgreSqlStorage(c => c.UseNpgsqlConnection(npgsqlConnectionString));
            }
            else
            {
                config.UseSqlServerStorage(connectionString, new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout       = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout  = TimeSpan.FromMinutes(5),
                    QueuePollInterval           = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks          = true
                });
            }
        });
        services.AddHangfireServer();

        // ISchedulerService — Hangfire-backed (replaces DevelopmentSchedulerService)
        services.AddScoped<ISchedulerService, HangfireSchedulerService>();

        // Register job classes so Hangfire's IoC activator can resolve them
        services.AddScoped<InvoiceOverdueJob>();
        services.AddScoped<MissedAppointmentJob>();

        // 7. Health Checks
        var healthChecks = services.AddHealthChecks();
        if (isPostgres)
        {
            healthChecks.AddNpgSql(npgsqlConnectionString, name: "postgres", tags: ["db", "infrastructure"]);
        }
        else
        {
            healthChecks.AddSqlServer(connectionString, name: "sql-server", tags: ["db", "infrastructure"]);
        }

        // Add Redis health check only when Redis is configured
        if (!string.IsNullOrWhiteSpace(redisConnStr))
        {
            healthChecks.AddRedis(redisConnStr, name: "redis", tags: ["cache", "infrastructure"]);
        }

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
