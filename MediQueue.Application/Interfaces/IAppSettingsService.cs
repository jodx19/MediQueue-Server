namespace MediQueue.Application.Interfaces;

/// <summary>
/// Provides application-wide settings to the Application layer
/// without creating a direct dependency on IConfiguration (which lives in the API/Infrastructure layer).
/// Implemented by Infrastructure's AppSettingsService.
/// </summary>
public interface IAppSettingsService
{
    /// <summary>The frontend base URL (e.g. https://app.mediqueue.com). Used to build email links.</summary>
    string FrontendUrl { get; }
}
