using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MediQueue.Application.Interfaces;

namespace MediQueue.API;

/// <summary>
/// Extension methods that register all Presentation-layer (API) services.
/// Called once from the Composition Root (Program.cs).
/// Does NOT reference SQL, EF Core, Redis, or Hangfire — those are Infrastructure concerns.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 1. JWT Authentication
        var jwtSection = configuration.GetSection("JwtSettings");
        var secretKey = jwtSection["SecretKey"] ?? throw new InvalidOperationException("JwtSettings:SecretKey is missing.");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSection["Issuer"] ?? "MediQueue",
                ValidAudience = jwtSection["Audience"] ?? "MediQueueClient",
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ClockSkew = TimeSpan.Zero
            };

            // Allow SignalR to receive token from query string
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                    {
                        context.Token = accessToken;
                    }
                    return Task.CompletedTask;
                }
            };
        });

        // 2. Controllers — thin layer, no business logic
        services.AddAuthorization(options =>
        {
            options.AddPolicy("SuperAdminOnly",        p => p.RequireRole("SuperAdmin"));
            options.AddPolicy("AdminOnly",             p => p.RequireRole("Admin"));
            options.AddPolicy("DoctorOnly",            p => p.RequireRole("Doctor"));
            options.AddPolicy("ReceptionistOnly",      p => p.RequireRole("Receptionist"));
            options.AddPolicy("StaffOnly",             p => p.RequireRole("Admin", "Doctor", "Receptionist"));
            options.AddPolicy("AdminOrReceptionist",    p => p.RequireRole("Admin", "Receptionist"));
            options.AddPolicy("PatientOnly",           p => p.RequireRole("Patient"));
            options.AddPolicy("AdminOrDoctor",          p => p.RequireRole("Admin", "Doctor"));
        });

        services.AddControllers(options =>
        {
            // Wrap every ObjectResult in ApiResponse<T> automatically
            options.Filters.Add<Middleware.ApiResponseFilter>();
        });
        services.AddEndpointsApiExplorer();

        // 2. Swagger / OpenAPI
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title       = "MediQueue EMR API",
                Version     = "v1",
                Description = "RESTful API for MediQueue Electronic Medical Records System",
                Contact     = new OpenApiContact
                {
                    Name = "MediQueue Team"
                }
            });

            // JWT Bearer authentication support in Swagger UI
            var jwtScheme = new OpenApiSecurityScheme
            {
                BearerFormat = "JWT",
                Name         = "Authorization",
                In           = ParameterLocation.Header,
                Type         = SecuritySchemeType.ApiKey,
                Scheme       = "Bearer",
                Description  = "Enter: Bearer {your JWT token}",
                Reference    = new OpenApiReference
                {
                    Id   = "Bearer",
                    Type = ReferenceType.SecurityScheme
                }
            };
            options.AddSecurityDefinition("Bearer", jwtScheme);
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { jwtScheme, Array.Empty<string>() }
            });

            // Fix for schema ID conflicts (e.g., RevenueReportDto in different namespaces)
            options.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
        });

        // 4. Real-time (SignalR)
        services.AddSignalR();

        // 5. HTTP context accessor (required by CurrentUserService)
        services.AddHttpContextAccessor();

        // 6. Register presentation-layer service implementations
        //    These implement Application interfaces but live in the API project
        //    because they depend on ASP.NET Core HTTP / SignalR infrastructure.
        services.AddScoped<ICurrentUserService, Services.CurrentUserService>();
        services.AddScoped<ITenantContext,      Services.TenantContext>();
        services.AddScoped<IRealtimeService,    Services.SignalRRealtimeService>();

        return services;
    }
}
