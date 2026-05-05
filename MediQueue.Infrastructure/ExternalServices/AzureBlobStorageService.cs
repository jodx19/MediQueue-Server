using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using MediQueue.Application.Interfaces;

namespace MediQueue.Infrastructure.ExternalServices;

public class AzureBlobStorageService : IStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;

    public AzureBlobStorageService(IConfiguration configuration)
    {
        var connectionString = configuration["Azure:StorageConnectionString"] ?? "UseDevelopmentStorage=true";
        _blobServiceClient = new BlobServiceClient(connectionString);
        _containerName = configuration["Azure:ContainerName"] ?? "mediqueue-blobs";
    }

    public async Task<string> UploadAsync(string fileName, Stream stream, string contentType)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        await containerClient.CreateIfNotExistsAsync(Azure.Storage.Blobs.Models.PublicAccessType.None);

        var blobClient = containerClient.GetBlobClient(fileName);
        
        var options = new Azure.Storage.Blobs.Models.BlobUploadOptions
        {
            HttpHeaders = new Azure.Storage.Blobs.Models.BlobHttpHeaders { ContentType = contentType }
        };

        await blobClient.UploadAsync(stream, options);

        return blobClient.Uri.ToString();
    }

    public async Task DeleteAsync(string fileUrl)
    {
        var uri = new Uri(fileUrl);
        var blobName = Path.GetFileName(uri.LocalPath);
        
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        await blobClient.DeleteIfExistsAsync();
    }

    public async Task<string> GetDownloadUrlAsync(string fileUrl, int expiryMinutes)
    {
        var uri = new Uri(fileUrl);
        var blobName = Path.GetFileName(uri.LocalPath);

        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        if (!await blobClient.ExistsAsync())
        {
            return string.Empty;
        }

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _containerName,
            BlobName = blobName,
            Resource = "b",
            StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
            ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(expiryMinutes)
        };
        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        var sasUri = blobClient.GenerateSasUri(sasBuilder);
        return sasUri.ToString();
    }
}
