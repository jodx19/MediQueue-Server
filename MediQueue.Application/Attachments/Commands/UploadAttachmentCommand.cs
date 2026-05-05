// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Attachments\Commands\UploadAttachmentCommand.cs
using System;
using System.IO;
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
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStorageService _storageService;

    public UploadAttachmentCommandHandler(IUnitOfWork unitOfWork, IStorageService storageService)
    {
        _unitOfWork = unitOfWork;
        _storageService = storageService;
    }

    public async Task<Result<Guid>> Handle(UploadAttachmentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var patient = await _unitOfWork.Patients.GetByIdAsync(request.PatientId);
            if (patient == null)
            {
                return Result<Guid>.Failure("Patient not found.");
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
}
