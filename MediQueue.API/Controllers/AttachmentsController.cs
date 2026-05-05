// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.API\Controllers\AttachmentsController.cs
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using MediatR;
using MediQueue.Application.Attachments.Commands;
using MediQueue.Domain.Entities;

namespace MediQueue.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiVersion("1.0")]
public class AttachmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AttachmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("upload")]
    public async Task<ActionResult<Guid>> Upload(
        [FromForm] Guid patientId, 
        [FromForm] IFormFile file, 
        [FromForm] AttachmentType type,
        [FromForm] Guid? clinicalVisitId = null,
        [FromForm] string? description = null)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        using var stream = file.OpenReadStream();
        var command = new UploadAttachmentCommand(
            patientId,
            file.FileName,
            stream,
            file.ContentType,
            file.Length,
            type,
            clinicalVisitId,
            description);

        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }
}
