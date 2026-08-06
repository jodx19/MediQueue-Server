using MediQueue.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace MediQueue.Infrastructure.Services;

/// <summary>
/// Provides app-wide settings to the Application layer, bridging IConfiguration
/// without leaking it across architectural boundaries.
/// </summary>
public class AppSettingsService : IAppSettingsService
{
    private readonly IConfiguration _configuration;

    public AppSettingsService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string FrontendUrl =>
        _configuration["ApiSettings:FrontendUrl"] ?? "http://localhost:4200";
}
