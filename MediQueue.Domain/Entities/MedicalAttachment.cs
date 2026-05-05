// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Domain\Entities\MedicalAttachment.cs
using System;
using MediQueue.Domain.Common;

namespace MediQueue.Domain.Entities;

public enum AttachmentType
{
    Imaging = 1,
    LabResult = 2,
    Prescription = 3,
    Other = 4
}

public class MedicalAttachment : BaseEntity
{
    public Guid PatientId { get; private set; }
    public Guid? ClinicalVisitId { get; private set; }
    public string FileName { get; private set; }
    public string FileUrl { get; private set; }
    public string ContentType { get; private set; }
    public long FileSize { get; private set; }
    public AttachmentType Type { get; private set; }
    public string? Description { get; private set; }
    public DateTime UploadedAt { get; private set; }

    private MedicalAttachment() 
    { 
        FileName = null!;
        FileUrl = null!;
        ContentType = null!;
    }

    private MedicalAttachment(
        Guid patientId, 
        string fileName, 
        string fileUrl, 
        string contentType, 
        long fileSize, 
        AttachmentType type,
        Guid? clinicalVisitId = null,
        string? description = null)
    {
        PatientId = patientId;
        ClinicalVisitId = clinicalVisitId;
        FileName = fileName;
        FileUrl = fileUrl;
        ContentType = contentType;
        FileSize = fileSize;
        Type = type;
        Description = description;
        UploadedAt = DateTime.UtcNow;
    }

    public static MedicalAttachment Create(
        Guid patientId, 
        string fileName, 
        string fileUrl, 
        string contentType, 
        long fileSize, 
        AttachmentType type,
        Guid? clinicalVisitId = null,
        string? description = null)
    {
        return new MedicalAttachment(patientId, fileName, fileUrl, contentType, fileSize, type, clinicalVisitId, description);
    }
}
