// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.API\Controllers\AttachmentsController.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using MediQueue.Application.Attachments.Commands;
using MediQueue.Application.Interfaces;
using MediQueue.Domain.Entities;
using MediQueue.Domain.Interfaces;

namespace MediQueue.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AttachmentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStorageService _storage;

    public AttachmentsController(
        IMediator mediator,
        IUnitOfWork unitOfWork,
        IStorageService storage)
    {
        _mediator   = mediator;
        _unitOfWork = unitOfWork;
        _storage    = storage;
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<Guid>> Upload(AttachmentUploadRequest request)
    {
        if (request.File == null || request.File.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        using var stream = request.File.OpenReadStream();
        var command = new UploadAttachmentCommand(
            request.PatientId,
            request.File.FileName,
            stream,
            request.File.ContentType,
            request.File.Length,
            request.Type,
            request.ClinicalVisitId,
            request.Description);

        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Generates a short-lived signed download URL for the given attachment
    /// and returns a 302 redirect to it.
    /// The client never receives the raw signed URL — the server redirects.
    /// </summary>
    [HttpGet("{id:guid}/download")]
    [Authorize]
    public async Task<ActionResult> Download(Guid id, CancellationToken ct)
    {
        var attachment = await _unitOfWork.Attachments.GetByIdAsync(id);
        if (attachment is null) return NotFound();

        // Generate a signed URL valid for 10 minutes
        var signedUrl = await _storage.GetDownloadUrlAsync(attachment.FileUrl, expiryMinutes: 10);

        // Return JSON with the download URL — frontend creates <a> tag
        return Ok(new
        {
            url      = signedUrl,
            fileName = attachment.FileName,
            type     = attachment.ContentType
        });
    }
}
