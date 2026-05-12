// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Server.Host\Program.cs
using System.Text;
using Hangfire;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using MediQueue.Application;
using MediQueue.API.Middleware;
using MediQueue.API.Hubs;
using MediQueue.API.Services;
using MediQueue.Application.Interfaces;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;
using Hangfire.Dashboard;
using MediQueue.Infrastructure;
using MediQueue.Infrastructure.Persistence;
using MediQueue.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ── Serilog ──────────────────────────────────────────────────────────────────
builder.Host.UseSerilog((ctx, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration));

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// ── Web/API Services (Presentation/Web layer concerns) ───────────────────────
builder.Services.AddHttpContextAccessor();
builder.Services.AddSignalR();
builder.Services.AddScoped<MediQueue.Application.Interfaces.ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<MediQueue.Application.Interfaces.IRealtimeService, SignalRRealtimeService>();

// ── Hangfire (SQL Server storage) ─────────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(connectionString));

builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = Environment.ProcessorCount * 2;
});

// ── Controllers (Reference the API assembly) ──────────────────────────────────
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ApiResponseFilter>();
})
    .AddApplicationPart(typeof(MediQueue.API.Controllers.BaseApiController).Assembly);

builder.Services.AddEndpointsApiExplorer();

// ── Rate Limiting ─────────────────────────────────────────────────────────────
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("global", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 100;
        opt.QueueLimit = 0;
    });

    options.AddFixedWindowLimiter("auth", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 5;
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
        ValidateIssuer           = false,
        // ValidIssuer              = jwtSettings["Issuer"] ?? "MediQueue",
        ValidateAudience         = false,
        // ValidAudience            = jwtSettings["Audience"] ?? "MediQueueClient",
        ValidateLifetime         = true,
        ClockSkew                = TimeSpan.Zero
    };

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

// ── CORS ──────────────────────────────────────────────────────────────────────
var angularOrigin = builder.Configuration["Cors:AllowedOrigin"] ?? "http://localhost:4200";
builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularPolicy", policy =>
        policy.WithOrigins(angularOrigin)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

// ── Swagger ───────────────────────────────────────────────────────────────────
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title       = "MediQueue EMR API",
        Version     = "v1",
        Description = "Multi-Specialty Clinic Electronic Medical Records System"
    });

    options.CustomSchemaIds(type => type.FullName);

    var scheme = new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme       = "bearer",
        BearerFormat = "JWT",
        In           = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description  = "Enter JWT token"
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
});

// ── Health Checks ─────────────────────────────────────────────────────────────
builder.Services.AddHealthChecks()
    .AddSqlServer(connectionString, name: "sqlserver", tags: ["db", "sql"]);

// ─────────────────────────────────────────────────────────────────────────────
var app = builder.Build();
// ─────────────────────────────────────────────────────────────────────────────

// ── Database Seeding ──────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<IDataSeeder>();
    await seeder.SeedAsync();
}

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseSerilogRequestLogging();

if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseCors("AngularPolicy");
app.UseSwagger();
app.UseSwaggerUI(opts =>
{
    opts.SwaggerEndpoint("/swagger/v1/swagger.json", "MediQueue EMR v1");
    opts.RoutePrefix = "swagger";
});

app.UseRateLimiter();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = app.Environment.IsDevelopment()
        ? [new Hangfire.Dashboard.LocalRequestsOnlyAuthorizationFilter()]
        : [new AdminHangfireAuthFilter()]
});

// ── Infrastructure Jobs & initialization ──────────────────────────────────────
app.UseInfrastructure();

app.MapHub<ClinicHub>("/hubs/clinic");
app.MapControllers();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

await app.RunAsync();

public class AdminHangfireAuthFilter : Hangfire.Dashboard.IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = ((AspNetCoreDashboardContext)context).HttpContext;
        return httpContext.User.Identity?.IsAuthenticated == true && httpContext.User.IsInRole("Admin");
    }
}
