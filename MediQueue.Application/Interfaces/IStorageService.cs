// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Interfaces\IStorageService.cs
using System.IO;
using System.Threading.Tasks;

namespace MediQueue.Application.Interfaces;

/// <summary>
/// Service for cloud storage operations (e.g. Azure Blob).
/// </summary>
public interface IStorageService
{
    Task<string> UploadAsync(string fileName, Stream stream, string contentType);
    Task DeleteAsync(string fileUrl);
    Task<string> GetDownloadUrlAsync(string fileUrl, int expiryMinutes);
}
