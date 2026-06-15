// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Attachments\Commands\UploadAttachmentCommand.cs
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Application.Interfaces;
using MediQueue.Domain.Entities;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.Attachments.Commands;

public record UploadAttachmentCommand(
    Guid PatientId,
    string FileName,
    Stream FileStream,
    string ContentType,
    long FileSize,
    AttachmentType Type,
    Guid? ClinicalVisitId = null,
    string? Description = null) : ICommand<Guid>;

public class UploadAttachmentCommandHandler : IRequestHandler<UploadAttachmentCommand, Result<Guid>>
{
    private static readonly Dictionary<string, byte[]> MagicBytes = new()
    {
        { "image/jpeg",      [0xFF, 0xD8, 0xFF] },
        { "image/png",       [0x89, 0x50, 0x4E, 0x47] },
        { "application/pdf", [0x25, 0x50, 0x44, 0x46] },
    };

    private readonly IUnitOfWork _unitOfWork;
    private readonly IStorageService _storageService;
    private readonly ICurrentUserService _currentUserService;

    public UploadAttachmentCommandHandler(
        IUnitOfWork unitOfWork,
        IStorageService storageService,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _storageService = storageService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(UploadAttachmentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // PatientId ownership check for Patient role
            if (_currentUserService.Role == "Patient" &&
                _currentUserService.PatientId != request.PatientId)
            {
                return Result<Guid>.Failure("You can only upload attachments for yourself.");
            }

            var patient = await _unitOfWork.Patients.GetByIdAsync(request.PatientId);
            if (patient == null)
            {
                return Result<Guid>.Failure("Patient not found.");
            }

            // Magic-byte verification
            if (!await ValidateMagicBytesAsync(request.FileStream, request.ContentType))
            {
                return Result<Guid>.Failure("File content does not match the declared file type.");
            }

            // Reset stream position after magic-byte reading
            if (request.FileStream.CanSeek)
            {
                request.FileStream.Position = 0;
            }

            // 1. Upload to cloud storage
            var fileUrl = await _storageService.UploadAsync(request.FileName, request.FileStream, request.ContentType);

            // 2. Create database record
            var attachment = MedicalAttachment.Create(
                request.PatientId,
                request.FileName,
                fileUrl,
                request.ContentType,
                request.FileSize,
                request.Type,
                request.ClinicalVisitId,
                request.Description);

            await _unitOfWork.Attachments.AddAsync(attachment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(attachment.Id);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Failure($"Failed to upload attachment: {ex.Message}");
        }
    }

    /// <summary>
    /// Validates that the file stream's header bytes match the declared content type.
    /// Reads up to 16 bytes (enough for WebP RIFF+WEBP signature) and resets position.
    /// </summary>
    private static async Task<bool> ValidateMagicBytesAsync(Stream stream, string contentType)
    {
        if (!stream.CanRead)
            return false;

        byte[] header = new byte[16];
        int bytesRead = 0;

        stream.Position = 0;
        while (bytesRead < header.Length)
        {
            int n = await stream.ReadAsync(header, bytesRead, header.Length - bytesRead);
            if (n == 0) break;
            bytesRead += n;
        }

        if (bytesRead < 4)
            return false;

        // WebP check: RIFF (bytes 0-3) + WEBP (bytes 8-11)
        if (contentType == "image/webp")
        {
            if (bytesRead < 12)
                return false;
            return header[0] == 0x52 && header[1] == 0x49 &&
                   header[2] == 0x46 && header[3] == 0x46 &&
                   header[8] == 0x57 && header[9] == 0x45 &&
                   header[10] == 0x42 && header[11] == 0x50;
        }

        // Standard magic bytes match
        if (MagicBytes.TryGetValue(contentType, out var expected))
        {
            if (bytesRead < expected.Length)
                return false;
            return header.Take(expected.Length).SequenceEqual(expected);
        }

        return false;
    }
}
