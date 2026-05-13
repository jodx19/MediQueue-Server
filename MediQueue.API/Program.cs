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

    // ── CORS Configuration ───────────────────────────────────────────────────
    builder.Services.AddCors(o => o.AddPolicy("Angular", p =>
        p.WithOrigins("http://localhost:4200")
         .AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials()));

    // ── Layer Registration ───────────────────────────────────────────────────
    builder.Services.AddApplicationServices();
    builder.Services.AddInfrastructureServices(builder.Configuration);
    builder.Services.AddApiServices(builder.Configuration);

    // ── Register DataSeeder ──────────────────────────────────────────────────
    builder.Services.AddScoped<IDataSeeder, MediQueue.Infrastructure.Persistence.DataSeeder>();

    var app = builder.Build();

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
