using System;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MediQueue.Application.Interfaces;

namespace MediQueue.API;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();

        // 1. JWT Authentication
        var jwtSettings = configuration.GetSection("Jwt");
        var secretKey = jwtSettings["Key"] ?? throw new InvalidOperationException("JWT SecretKey is missing.");

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
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ClockSkew = TimeSpan.Zero
            };

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
                    return System.Threading.Tasks.Task.CompletedTask;
                }
            };
        });

        // 2. Authorization Policies
        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
            options.AddPolicy("DoctorOnly", p => p.RequireRole("Doctor"));
            options.AddPolicy("ReceptionistOnly", p => p.RequireRole("Receptionist"));
            options.AddPolicy("StaffOnly", p => p.RequireRole("Admin", "Doctor", "Receptionist"));
            options.AddPolicy("AdminOrReceptionist", p => p.RequireRole("Admin", "Receptionist"));
            options.AddPolicy("PatientOnly", p => p.RequireRole("Patient"));
            options.AddPolicy("AdminOrDoctor", p => p.RequireRole("Admin", "Doctor"));
        });

        // 3. Swagger / OpenAPI
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title       = "MediQueue EMR API",
                Version     = "v1",
                Description = "RESTful API for MediQueue Electronic Medical Records System",
                Contact     = new OpenApiContact { Name = "MediQueue Team" }
            });

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

            options.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
        });

        // 4. CORS
        var allowedOrigin = configuration["Cors:AllowedOrigin"] ?? "http://localhost:4200";
        services.AddCors(opts =>
        {
            opts.AddDefaultPolicy(policy => policy
                .WithOrigins(allowedOrigin)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());
        });

        // 5. SignalR
        services.AddSignalR();

        // 6. HTTP Context Accessor
        services.AddHttpContextAccessor();

        // 7. Presentation-layer services
        services.AddScoped<ICurrentUserService, Services.CurrentUserService>();
        services.AddScoped<IRealtimeService, Services.SignalRRealtimeService>();

        return services;
    }
}
