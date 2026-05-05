using System;
using Microsoft.AspNetCore.Http;
using MediQueue.Domain.Entities;

namespace MediQueue.API.Controllers;

public class AttachmentUploadRequest
{
    public Guid PatientId { get; set; }
    public IFormFile File { get; set; } = null!;
    public AttachmentType Type { get; set; }
    public Guid? ClinicalVisitId { get; set; }
    public string? Description { get; set; }
}
