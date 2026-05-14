using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediQueue.Application.ClinicalVisits.DTOs;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.ClinicalVisits;

internal static class ClinicalVisitAttachmentMapper
{
    public static async Task PopulateAttachmentsAsync(
        IUnitOfWork unitOfWork,
        ClinicalVisitDetailDto dto,
        Guid visitId,
        CancellationToken cancellationToken)
    {
        var files = await unitOfWork.Attachments.GetByClinicalVisitIdAsync(visitId, cancellationToken);
        dto.Attachments = files.Select(f => new AttachmentDto
        {
            Id = f.Id,
            FileName = f.FileName,
            FileUrl = f.FileUrl,
            ContentType = f.ContentType,
            FileSize = f.FileSize,
            Type = f.Type.ToString(),
            Description = f.Description,
            UploadedAt = f.UploadedAt,
        }).ToList();
    }
}
