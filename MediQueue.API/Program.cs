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

        // Validate JWT Secret
        var jwtSecret = config["JwtSettings:SecretKey"];
        if (string.IsNullOrEmpty(jwtSecret))
            throw new InvalidOperationException(
                "❌ CRITICAL: JwtSettings:SecretKey is not configured.\n" +
                "   Set via environment variable: set JwtSettings__SecretKey=<32+ chars>");

        if (jwtSecret.Contains("REPLACE_WITH"))
            throw new InvalidOperationException(
                "❌ CRITICAL: JwtSettings:SecretKey still contains REPLACE_WITH placeholder.");

        if (jwtSecret.Length < 32)
            throw new InvalidOperationException(
                $"❌ CRITICAL: JwtSettings:SecretKey must be at least 32 characters. Current: {jwtSecret.Length}");

        Console.WriteLine("   ✅ JWT Secret validated");

        // Validate SQL Connection String
        var sqlConnStr = config.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(sqlConnStr))
            throw new InvalidOperationException(
                "❌ CRITICAL: ConnectionStrings:DefaultConnection is not configured.");

        if (sqlConnStr.Contains("REPLACE_WITH"))
            throw new InvalidOperationException(
                "❌ CRITICAL: SQL Connection String contains REPLACE_WITH placeholder.");

        if (sqlConnStr.Contains("localhost") || sqlConnStr.Contains("localdb") || sqlConnStr.Contains("(localdb)"))
            throw new InvalidOperationException(
                "❌ CRITICAL: SQL Connection String contains dev values (localhost/localdb).\n" +
                "   Use production SQL Server hostname.");

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
                "❌ CRITICAL: AzureStorage:ConnectionString contains REPLACE_WITH placeholder.");

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

    // ── Database Schema Initialization & Seed ────────────────────────────────
    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var db = scope.ServiceProvider.GetRequiredService<ClinicDbContext>();
            await db.Database.EnsureCreatedAsync();
            Log.Information("Database schema ensured.");

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
    app.UseMiddleware<GlobalExceptionMiddleware>();
    app.UseSerilogRequestLogging();

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MediQueue EMR API v1");
        c.RoutePrefix = "swagger";
    });

    app.UseCors("Angular");
    app.UseStaticFiles();
    app.UseRouting();
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


    RecurringJob.AddOrUpdate<InvoiceOverdueJob>(
        "invoice-overdue-daily",
        job => job.ExecuteAsync(),
        Cron.Daily);

    RecurringJob.AddOrUpdate<MissedAppointmentJob>(
        "missed-appointments-daily",
        job => job.ExecuteAsync(),
        Cron.Daily);

    // ── Endpoints ────────────────────────────────────────────────────────────
    app.MapHub<ClinicHub>("/hubs/clinic");
    app.MapControllers();
    app.MapHealthChecks("/health");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "MediQueue API terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}

// Expose Program for WebApplicationFactory in integration tests.
public partial class Program { }
