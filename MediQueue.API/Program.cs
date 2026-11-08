using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;
using MediQueue.Application;
using MediQueue.Application.Interfaces;
using MediQueue.Infrastructure;
using MediQueue.API;
using MediQueue.API.Middleware;
using MediQueue.API.Hubs;
using Microsoft.EntityFrameworkCore;
using MediQueue.Infrastructure.Persistence.Context;
using Hangfire;
using Hangfire.Dashboard;
using MediQueue.Infrastructure.ExternalServices;


// ── Bootstrap Logging ────────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ── Serilog ──────────────────────────────────────────────────────────────
    builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));

    // ── CORS Configuration (subdomain-aware) ──────────────────────────────
    var allowedOrigins = builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>() ?? [];

    var exactOrigins = allowedOrigins
        .Where(o => !o.Contains('*'))
        .ToArray();

    var wildcardSuffixes = allowedOrigins
        .Where(o => o.Contains('*'))
        .Select(o => o.Replace("https://", "")
                      .Replace("http://", "")
                      .Replace("*.", ""))
        .ToArray();

    builder.Services.AddCors(o => o.AddPolicy("Angular", p =>
    {
        p.AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials()
         .SetIsOriginAllowed(origin =>
         {
             if (string.IsNullOrEmpty(origin))
                 return false;

             // Dev: allow localhost
             if (origin.StartsWith("http://localhost", StringComparison.OrdinalIgnoreCase) ||
                 origin.StartsWith("http://127.0.0.1", StringComparison.OrdinalIgnoreCase))
                 return true;

             // Exact match
             if (exactOrigins.Contains(origin))
                 return true;

             // Wildcard subdomain match
             var uri = new Uri(origin);
             return wildcardSuffixes.Any(pattern =>
                 uri.Host.EndsWith("." + pattern, StringComparison.OrdinalIgnoreCase) ||
                 uri.Host.Equals(pattern, StringComparison.OrdinalIgnoreCase));
         });
    }));

    // ── Layer Registration ───────────────────────────────────────────────────
    builder.Services.AddApplicationServices();
    builder.Services.AddInfrastructureServices(builder.Configuration);
    builder.Services.AddApiServices(builder.Configuration);

    // ── Rate Limiting (Per-IP Partitioned) ───────────────────────────────────
    // Using PartitionedRateLimiter ensures each IP gets its OWN window.
    // With a shared FixedWindowLimiter, 10 different users exhaust the global
    // quota — and one attacker is not properly isolated from legitimate users.
    builder.Services.AddRateLimiter(options =>
    {
        var rlConfig = builder.Configuration.GetSection("RateLimiting");

        // ── Auth (Login) — 10 req/min per IP ────────────────────────────────
        options.AddPolicy("AuthPolicy", httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit       = rlConfig.GetValue<int>("AuthPolicy:PermitLimit", 10),
                    Window            = TimeSpan.FromMinutes(rlConfig.GetValue<int>("AuthPolicy:WindowMinutes", 1)),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit        = 0,
                }));

        // ── Patient Login — 5 req/min per IP ────────────────────────────────
        options.AddPolicy("PatientLoginPolicy", httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit       = rlConfig.GetValue<int>("PatientLoginPolicy:PermitLimit", 5),
                    Window            = TimeSpan.FromMinutes(rlConfig.GetValue<int>("PatientLoginPolicy:WindowMinutes", 1)),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit        = 0,
                }));

        // ── Register — 5 req/min per IP ─────────────────────────────────────
        // Prevents account-spam and email-enumeration via the register endpoint.
        options.AddPolicy("RegisterPolicy", httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit       = rlConfig.GetValue<int>("RegisterPolicy:PermitLimit", 5),
                    Window            = TimeSpan.FromMinutes(rlConfig.GetValue<int>("RegisterPolicy:WindowMinutes", 1)),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit        = 0,
                }));

        // ── ForgotPassword — 3 req/5 min per IP ─────────────────────────────
        // Tightest policy — this endpoint is the primary vector for
        // account-enumeration attacks; low limit + longer window.
        options.AddPolicy("ForgotPasswordPolicy", httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit       = rlConfig.GetValue<int>("ForgotPasswordPolicy:PermitLimit", 3),
                    Window            = TimeSpan.FromMinutes(rlConfig.GetValue<int>("ForgotPasswordPolicy:WindowMinutes", 5)),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit        = 0,
                }));

        // ── 429 response — matches ApiResponse<T> shape ──────────────────────
        // Angular api-response.interceptor reads response.data
        options.OnRejected = async (context, cancellationToken) =>
        {
            context.HttpContext.Response.StatusCode = 429;
            context.HttpContext.Response.ContentType = "application/json";
            await context.HttpContext.Response.WriteAsJsonAsync(
                new
                {
                    isSuccess = false,
                    data      = (object?)null,
                    message   = "Too many requests. Please try again later.",
                    errors    = new[] { "Rate limit exceeded" }
                },
                cancellationToken
            );
        };
    });

    // ── Register DataSeeder ──────────────────────────────────────────────────
    builder.Services.AddScoped<IDataSeeder, MediQueue.Infrastructure.Persistence.DataSeeder>();

    var app = builder.Build();

    // ── Cache implementation banner ───────────────────────────────────────────
    {
        var redisCheck = builder.Configuration.GetConnectionString("Redis");
        var cacheImpl  = !string.IsNullOrWhiteSpace(redisCheck) ? "RedisCacheService" : "MemoryCacheService";
        Log.Information("ICacheService implementation: {CacheImpl} (Redis connection string {Status})",
            cacheImpl,
            !string.IsNullOrWhiteSpace(redisCheck) ? $"present ({redisCheck})" : "absent");
    }

    // ── HTTPS Enforcement ────────────────────────────────────────────────────
    // UseHsts: Production only (dev uses HTTP for local debugging)
    if (!app.Environment.IsDevelopment())
    {
        app.UseHsts();
        app.UseHttpsRedirection();
        // Security response headers (OWASP) — only in non-dev to avoid
        // CSP conflicts with Swagger UI and hot-reload in development.
        app.UseSecurityHeaders();
    }

    // ── Validate Critical Configuration ──────────────────────────────────────
    var env = builder.Environment;
    var config = builder.Configuration;

    Console.WriteLine($"\n🔧 Starting MediQueue in {env.EnvironmentName} mode...\n");

    if (env.IsProduction())
    {
        Console.WriteLine("🔍 Validating Production Configuration...");

        // Validate JWT Secret — FAIL FAST (a silent fallback secret is a security incident)
        var jwtSecret = config["JwtSettings:SecretKey"];
        if (string.IsNullOrEmpty(jwtSecret) || jwtSecret.Contains("REPLACE_WITH") || jwtSecret.StartsWith("${") || jwtSecret.Length < 32)
        {
            throw new InvalidOperationException(
                "❌ FATAL: JwtSettings:SecretKey is missing, a placeholder, or shorter than 32 chars. " +
                "Set the JwtSettings__SecretKey environment variable (32+ random characters) and restart. " +
                "The app will NOT start with an insecure fallback secret.");
        }
        Console.WriteLine("   ✅ JWT Secret validated");

        // Validate SQL Connection String — FAIL FAST (no hardcoded credentials in source)
        var sqlConnStr = config.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(sqlConnStr) || sqlConnStr.StartsWith("${") || sqlConnStr.Contains("REPLACE_WITH"))
        {
            throw new InvalidOperationException(
                "❌ FATAL: ConnectionStrings:DefaultConnection is missing or a placeholder. " +
                "Set the ConnectionStrings__DefaultConnection environment variable " +
                "(e.g. postgresql://user:pass@host/db?sslmode=require) and restart.");
        }
        Console.WriteLine("   ✅ SQL Connection String validated");

        // Validate Redis Connection String (Optional — falls back to MemoryCache)
        var redisConnStr = config.GetConnectionString("Redis");
        if (string.IsNullOrEmpty(redisConnStr) || redisConnStr.Contains("localhost"))
        {
            Console.WriteLine("   ⚠️  Redis not configured or localhost — using in-memory cache fallback");
        }
        else
        {
            Console.WriteLine("   ✅ Redis Connection String validated");
        }

        // Validate Email Configuration (Optional — runs in fallback mode if credentials missing)
        var emailHost = config["EmailSettings:SmtpServer"];
        var emailUser = config["EmailSettings:Username"];
        var emailPass = config["EmailSettings:Password"];

        if (string.IsNullOrEmpty(emailHost) || string.IsNullOrEmpty(emailUser) || string.IsNullOrEmpty(emailPass))
        {
            Console.WriteLine("   ⚠️  Email not fully configured — running in fallback mode");
        }
        else
        {
            Console.WriteLine("   ✅ Email Configuration validated");
        }

        // Validate Azure Storage Configuration
        var azureConnStr = config["AzureStorage:ConnectionString"];
        if (!string.IsNullOrEmpty(azureConnStr) && azureConnStr.Contains("REPLACE_WITH"))
            throw new InvalidOperationException(
                "❌ FATAL: AzureStorage:ConnectionString contains a REPLACE_WITH placeholder. " +
                "Either set a real Azure Storage connection string (Storage account → Access keys) " +
                "or REMOVE the variable entirely to fall back to the local StubStorageService.");

        if (!string.IsNullOrEmpty(azureConnStr))
            Console.WriteLine("   ✅ Azure Storage Configuration validated");

        // Validate SeedingSettings
        var seedingEnabled = config.GetValue<bool>("SeedingSettings:EnableSeeding");
        if (seedingEnabled)
            Console.WriteLine("   ⚠️  WARNING: SeedingSettings:EnableSeeding is true in Production! Set to false.");

        Console.WriteLine("\n✅ ALL PRODUCTION VALIDATION CHECKS PASSED!\n");
    }
    else if (env.IsDevelopment())
    {
        Console.WriteLine("✅ Development mode: Using local defaults\n");
    }
    else
    {
        Console.WriteLine("⚠️  Staging mode: Verify all configuration values\n");
    }

    // ── Database Connectivity Preflight ─────────────────────────────────────
    // Hangfire storage + EF migrations both need the database. If it is
    // unreachable, fail with a CLEAR message instead of an obscure
    // "CLR worker thread exited prematurely" inside the IIS worker process.
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ClinicDbContext>();
        var canConnect = false;
        try
        {
            canConnect = await db.Database.CanConnectAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Database connectivity probe threw an exception.");
        }

        if (!canConnect)
        {
            var target = config.GetConnectionString("DefaultConnection") ?? "<null>";
            // Never print credentials — show host/database only.
            var safeTarget = target;
            var uriMatch = System.Text.RegularExpressions.Regex.Match(
                target, @"^[a-z]+://[^/@:]+:[^@]+@([^/]+)/([^?]*)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (uriMatch.Success)
                safeTarget = $"host={uriMatch.Groups[1].Value}, database={uriMatch.Groups[2].Value}";

            var message =
                "❌ FATAL: Cannot connect to the database. Hangfire and EF migrations require it. " +
                $"Connection target: {safeTarget}. " +
                "Check: 1) the connection string env var, 2) database server firewall/IP allow-list " +
                "(Neon: allow the hosting server IP), 3) database is not suspended (Neon free tier scales to zero).";

            if (env.IsProduction())
            {
                Log.Fatal(message);
                Console.WriteLine(message);
                Environment.ExitCode = 1;
                Log.CloseAndFlush();
                return; // fail fast — do not start a half-working app
            }
            Log.Warning(message + " (continuing in Development)");
        }
    }

    // ── Database Schema Initialization & Seed ────────────────────────────────
    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var db = scope.ServiceProvider.GetRequiredService<ClinicDbContext>();
            // Use EnsureCreated for SQL Server (no SQL Server migrations exist — schema built from model).
            // Use MigrateAsync for PostgreSQL (Npgsql migrations are present).
            if (db.Database.IsNpgsql())
            {
                await db.Database.MigrateAsync();
                Log.Information("PostgreSQL: Database migrated successfully.");
            }
            else
            {
                await db.Database.EnsureCreatedAsync();
                Log.Information("SQL Server: Database schema ensured (EnsureCreated).");
            }

            var seeder = scope.ServiceProvider.GetRequiredService<IDataSeeder>();
            await seeder.SeedAsync();
            Log.Information("Seed data check complete.");
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Database schema/seed step encountered an issue — continuing startup.");
        }
    }

    // ── Middleware Pipeline ──────────────────────────────────────────────────
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
    });

    app.UseMiddleware<GlobalExceptionMiddleware>();
    app.UseSerilogRequestLogging();

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MediQueue EMR API v1");
        c.RoutePrefix = "swagger";
    });

    app.UseRouting();
    app.UseCors("Angular");
    app.UseStaticFiles();
    app.UseMiddleware<TenantResolutionMiddleware>();
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();

    // ── Hangfire Dashboard & Recurring Jobs ──────────────────────────────────
    // Dashboard is protected by Basic Auth in Production (open in Development)
    var hangfireUser = builder.Configuration["Hangfire:DashboardUsername"] ?? "admin";
    var hangfirePass = builder.Configuration["Hangfire:DashboardPassword"] ?? "admin123!";

    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = [new MediQueue.API.Middleware.HangfireBasicAuthFilter(hangfireUser, hangfirePass)]
    });


    // Recurring job registration touches the Hangfire storage (database).
    // If the DB is unreachable at this moment, do NOT crash the whole app —
    // the API can still serve requests; jobs will re-register on next start.
    try
    {
        RecurringJob.AddOrUpdate<InvoiceOverdueJob>(
            "invoice-overdue-daily",
            job => job.ExecuteAsync(),
            Cron.Daily);

        RecurringJob.AddOrUpdate<MissedAppointmentJob>(
            "missed-appointments-daily",
            job => job.ExecuteAsync(),
            Cron.Daily);

        Log.Information("Hangfire recurring jobs registered.");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Failed to register Hangfire recurring jobs — the API will continue without them.");
    }

    // ── Endpoints ────────────────────────────────────────────────────────────
    app.MapHub<ClinicHub>("/hubs/clinic");
    app.MapControllers();
    app.MapGet("/health", () => Results.Ok(new { status = "Healthy", timestamp = DateTime.UtcNow }));
    app.MapHealthChecks("/health/ready");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "MediQueue API terminated unexpectedly.");
    // CRITICAL: without this, the process exits with code 0 and IIS reports
    // "exited from Program.Main with exit code = '0'" — hiding the real failure.
    // A non-zero code makes the crash visible to IIS/WAS and monitoring.
    Environment.ExitCode = 1;
}
finally
{
    Log.CloseAndFlush();
}

// Expose Program for WebApplicationFactory in integration tests.
public partial class Program { }
