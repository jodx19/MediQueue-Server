using MediQueue.Application.Interfaces;

namespace MediQueue.Infrastructure.ExternalServices;

/// <summary>
/// No-op storage service used when Azure Blob Storage is not configured.
/// Prevents startup failure on IIS hosting environments where Azure is optional.
/// </summary>
public class StubStorageService : IStorageService
{
    public Task<string> UploadAsync(string fileName, Stream stream, string contentType)
    {
        // Return a placeholder URL — uploads are not persisted without Azure configured
        return Task.FromResult($"/uploads/{fileName}");
    }

    public Task DeleteAsync(string fileUrl)
    {
        return Task.CompletedTask;
    }

    public Task<string> GetDownloadUrlAsync(string fileUrl, int expiryMinutes)
    {
        return Task.FromResult(fileUrl);
    }
}
