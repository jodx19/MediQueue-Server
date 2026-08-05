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

    // ── Rate Limiting ────────────────────────────────────────────────────────
    builder.Services.AddRateLimiter(options =>
    {
        var authPolicyConfig = builder.Configuration
            .GetSection("RateLimiting:AuthPolicy");

        options.AddFixedWindowLimiter("AuthPolicy", limiterOptions =>
        {
            limiterOptions.PermitLimit =
                authPolicyConfig.GetValue<int>("PermitLimit", 10);
            limiterOptions.Window = TimeSpan.FromMinutes(
                authPolicyConfig.GetValue<int>("WindowMinutes", 1));
            limiterOptions.QueueProcessingOrder =
                QueueProcessingOrder.OldestFirst;
            limiterOptions.QueueLimit = 0;
        });

        options.AddFixedWindowLimiter("PatientLoginPolicy", limiterOptions =>
        {
            limiterOptions.PermitLimit = 5;
            limiterOptions.Window = TimeSpan.FromMinutes(1);
            limiterOptions.QueueProcessingOrder =
                QueueProcessingOrder.OldestFirst;
            limiterOptions.QueueLimit = 0;
        });

        // 429 response MUST match ApiResponse<T> shape
        // Angular api-response.interceptor reads response.data
        options.OnRejected = async (context, cancellationToken) =>
        {
            context.HttpContext.Response.StatusCode = 429;
            context.HttpContext.Response.ContentType = "application/json";
            await context.HttpContext.Response.WriteAsJsonAsync(
                new
                {
                    isSuccess = false,
                    data = (object?)null,
                    message = "Too many requests. Please try again later.",
                    errors = new[] { "Rate limit exceeded" }
                },
                cancellationToken
            );
        };
    });

    // ── Register DataSeeder ──────────────────────────────────────────────────
    builder.Services.AddScoped<IDataSeeder, MediQueue.Infrastructure.Persistence.DataSeeder>();

    var app = builder.Build();

    // ── HTTPS Enforcement ────────────────────────────────────────────────────
    // UseHsts: Production only (dev uses HTTP for local debugging)
    if (!app.Environment.IsDevelopment())
    {
        app.UseHsts();
    }

    // UseHttpsRedirection: all environments, honors HTTPS ports from config/ENV
    app.UseHttpsRedirection();

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

        // Validate Redis Connection String
        var redisConnStr = config.GetConnectionString("Redis");
        if (string.IsNullOrEmpty(redisConnStr))
            throw new InvalidOperationException(
                "❌ CRITICAL: ConnectionStrings:Redis is not configured.");

        if (redisConnStr.Contains("localhost"))
            throw new InvalidOperationException(
                "❌ CRITICAL: Redis Connection String contains localhost.\n" +
                "   Use production Redis hostname.");

        Console.WriteLine("   ✅ Redis Connection String validated");

        // Validate Email Configuration (key names match EmailNotificationService.cs)
        var emailHost = config["EmailSettings:SmtpServer"];
        var emailUser = config["EmailSettings:Username"];
        var emailPass = config["EmailSettings:Password"];

        if (string.IsNullOrEmpty(emailHost) || emailHost.Contains("REPLACE_WITH"))
            throw new InvalidOperationException(
                "❌ CRITICAL: EmailSettings:SmtpServer is not configured.");

        if (string.IsNullOrEmpty(emailUser) || emailUser.Contains("REPLACE_WITH"))
            throw new InvalidOperationException(
                "❌ CRITICAL: EmailSettings:Username is not configured.");

        if (string.IsNullOrEmpty(emailPass) || emailPass.Contains("REPLACE_WITH"))
            throw new InvalidOperationException(
                "❌ CRITICAL: EmailSettings:Password is not configured.");

        Console.WriteLine("   ✅ Email Configuration validated");

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

    // ── Auto-migrate & Seed (Development only) ───────────────────────────────
    // Production: migrations are applied manually or via CI/CD pipeline
    //             (e.g. `dotnet ef database update` or SQL bundle runner).
    // Running MigrateAsync on startup in Production risks concurrent-instance
    // race conditions and locks the app into a DB dependency for boot.
    if (app.Environment.IsDevelopment())
    {
        using var scope = app.Services.CreateScope();
        try
        {
            var db = scope.ServiceProvider.GetRequiredService<ClinicDbContext>();
            await db.Database.MigrateAsync();
            Log.Information("Database migrations applied.");

            var seeder = scope.ServiceProvider.GetRequiredService<IDataSeeder>();
            await seeder.SeedAsync();
            Log.Information("Seed data check complete.");
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Migration/Seed step encountered an issue — continuing startup.");
        }
    }

    // ── Middleware Pipeline ──────────────────────────────────────────────────
    app.UseMiddleware<GlobalExceptionMiddleware>();
    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "MediQueue EMR API v1");
            c.RoutePrefix = "swagger";
        });
    }

    app.UseCors("Angular");
    app.UseStaticFiles();
    app.UseRouting();
    app.UseMiddleware<TenantResolutionMiddleware>();
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();

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
