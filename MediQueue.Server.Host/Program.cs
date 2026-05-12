// MediQueue.Server.Host — Composition Root
using Serilog;
using MediQueue.Application;
using MediQueue.Infrastructure;
using MediQueue.API;
using MediQueue.API.Middleware;
using MediQueue.API.Hubs;

// ── Bootstrap Logging ────────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ── Serilog ──────────────────────────────────────────────────────────────
    builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));

    // ── Layer Registration ───────────────────────────────────────────────────
    builder.Services.AddApplicationServices();                                
    builder.Services.AddInfrastructureServices(builder.Configuration);       
    builder.Services.AddApiServices(builder.Configuration);                  

    var app = builder.Build();

    // ── Middleware Pipeline ──────────────────────────────────────────────────
    app.UseMiddleware<GlobalExceptionMiddleware>();  
    app.UseSerilogRequestLogging();

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MediQueue EMR API v1");
        c.RoutePrefix = "swagger";
    });

    app.UseCors();                  
    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();

    // ── Infrastructure Startup ───────────────────────────────────────────────
    await app.UseInfrastructure();

    // ── Endpoints ────────────────────────────────────────────────────────────
    app.MapHub<ClinicHub>("/hubs/clinic");
    app.MapControllers();
    app.MapHealthChecks("/health");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "MediQueue Host terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}
