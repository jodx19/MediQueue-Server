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

    // ── Validate JWT Secret ──────────────────────────────────────────────────
    var jwtSecret = builder.Configuration["JwtSettings:SecretKey"];
    if (string.IsNullOrEmpty(jwtSecret) || jwtSecret.Length < 32)
    {
        throw new InvalidOperationException(
            "JwtSettings:SecretKey must be at least 32 characters long. " +
            "Set it via User Secrets (dev) or environment variables (prod).");
    }
    if (jwtSecret is "REPLACE_WITH_SECRET_KEY_MIN_32_CHARS"
                  or "MediQueue-Super-Secret-Key-256bit-2026!"
                  or "REPLACE_WITH_PRODUCTION_SECRET_KEY_MIN_32_CHARS")
    {
        throw new InvalidOperationException(
            "JwtSettings:SecretKey contains a placeholder value. " +
            "Set a real secret via User Secrets, environment variables, or Key Vault.");
    }

    // ── Auto-migrate & Seed ──────────────────────────────────────────────────
    using (var scope = app.Services.CreateScope())
    {
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
