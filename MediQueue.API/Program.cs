// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.API\Program.cs
using System.Text;
using Hangfire;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using MediQueue.Application;
using MediQueue.API.Middleware;
using MediQueue.Infrastructure;
using MediQueue.Infrastructure.Hubs;
using MediQueue.Infrastructure.ExternalServices;
using MediQueue.Infrastructure.Persistence;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;
using Hangfire.Dashboard;

var builder = WebApplication.CreateBuilder(args);

// ── Serilog ──────────────────────────────────────────────────────────────────
builder.Host.UseSerilog((ctx, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration));

// ── Application + Infrastructure layers ──────────────────────────────────────
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// ── Controllers ───────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ── Rate Limiting ─────────────────────────────────────────────────────────────
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("FixedPolicy", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 100;
        opt.QueueLimit = 10;
        opt.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
    });

    options.AddFixedWindowLimiter("AuthPolicy", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 10;
        opt.QueueLimit = 0;
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// ── JWT Bearer Authentication ─────────────────────────────────────────────────
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey   = jwtSettings["SecretKey"]
    ?? throw new InvalidOperationException("JwtSettings:SecretKey is required.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ValidateIssuer           = true,
        ValidIssuer              = jwtSettings["Issuer"] ?? "MediQueue",
        ValidateAudience         = true,
        ValidAudience            = jwtSettings["Audience"] ?? "MediQueueClient",
        ValidateLifetime         = true,
        ClockSkew                = TimeSpan.Zero
    };

    // Allow SignalR to pass the token in the query string
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                context.Token = accessToken;
            return Task.CompletedTask;
        }
    };
});

// ── Authorization Policies ────────────────────────────────────────────────────
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly",        p => p.RequireRole("Admin"));
    options.AddPolicy("DoctorOnly",       p => p.RequireRole("Doctor"));
    options.AddPolicy("PatientOrDoctor",  p => p.RequireRole("Patient", "Doctor", "Admin"));
});

// ── CORS — Angular dev origin ─────────────────────────────────────────────────
var angularOrigin = builder.Configuration["Cors:AllowedOrigin"] ?? "http://localhost:4200";
builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularPolicy", policy =>
        policy.WithOrigins(angularOrigin)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());  // required for SignalR cookies
});

// ── Swagger / OpenAPI ─────────────────────────────────────────────────────────
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title       = "MediQueue EMR API",
        Version     = "v1",
        Description = "Multi-Specialty Clinic Electronic Medical Records System"
    });

    // JWT Bearer in Swagger UI
    var scheme = new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme       = "bearer",
        BearerFormat = "JWT",
        In           = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description  = "Enter JWT token (without 'Bearer ' prefix)"
    };
    options.AddSecurityDefinition("Bearer", scheme);
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // XML comments for all doc'd endpoints
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
});

// ── Health Checks — SQL Server + Redis + Hangfire ─────────────────────────────
builder.Services.AddHealthChecks()
    .AddSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")!,
        name: "sqlserver",
        tags: ["db", "sql"])
    .AddRedis(
        builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379",
        name: "redis",
        tags: ["cache"])
    .AddHangfire(
        options => { options.MinimumAvailableServers = 1; },
        name: "hangfire",
        tags: ["jobs"]);

// ─────────────────────────────────────────────────────────────────────────────
var app = builder.Build();
// ─────────────────────────────────────────────────────────────────────────────

// ── Database Seeding (Development Only) ───────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<IDataSeeder>();
    await seeder.SeedAsync();
}

// ── Global Exception Middleware (must be first) ───────────────────────────────
app.UseMiddleware<GlobalExceptionMiddleware>();

// ── Serilog request logging ───────────────────────────────────────────────────
app.UseSerilogRequestLogging();

// ── HTTPS + CORS ──────────────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseCors("AngularPolicy");

// ── Swagger (all environments for developer convenience; lock down in prod via config) ──
app.UseSwagger();
app.UseSwaggerUI(opts =>
{
    opts.SwaggerEndpoint("/swagger/v1/swagger.json", "MediQueue EMR v1");
    opts.RoutePrefix = "swagger";
    opts.DocumentTitle = "MediQueue EMR API";
    opts.DefaultModelsExpandDepth(-1);
    opts.DisplayRequestDuration();
});

app.UseRateLimiter();
app.UseRouting();

// ── Auth ──────────────────────────────────────────────────────────────────────
app.UseAuthentication();
app.UseAuthorization();

// ── Hangfire Dashboard ────────────────────────────────────────────────────────
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    // Require authenticated admin in production
    Authorization = app.Environment.IsDevelopment()
        ? [new Hangfire.Dashboard.LocalRequestsOnlyAuthorizationFilter()]
        : [new AdminHangfireAuthFilter()]
});

// Schedule recurring jobs
using (var scope = app.Services.CreateScope())
{
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
    recurringJobManager.AddOrUpdate<DashboardJobs>(
        "daily-revenue-report", 
        job => job.SendDailyRevenueReportAsync(), 
        Cron.Daily);

    recurringJobManager.AddOrUpdate<MissedAppointmentJob>(
        "check-missed-appointments",
        job => job.ExecuteAsync(),
        "*/15 * * * *"); // Every 15 minutes

    recurringJobManager.AddOrUpdate<InvoiceOverdueJob>(
        "check-overdue-invoices",
        job => job.ExecuteAsync(),
        Cron.Daily);
}

// ── SignalR Hub ───────────────────────────────────────────────────────────────
app.MapHub<ClinicHub>("/hubs/clinic");

// ── Controllers ───────────────────────────────────────────────────────────────
app.MapControllers();

// ── Health Checks ─────────────────────────────────────────────────────────────
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false  // liveness: always 200 if process is up
});

await app.RunAsync();

// ── Hangfire auth filter for production ───────────────────────────────────────
public class AdminHangfireAuthFilter : Hangfire.Dashboard.IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = ((AspNetCoreDashboardContext)context).HttpContext;
        return httpContext.User.Identity?.IsAuthenticated == true && httpContext.User.IsInRole("Admin");
    }
}
