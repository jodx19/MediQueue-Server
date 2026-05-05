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
public class AttachmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AttachmentsController(IMediator mediator)
    {
        _mediator = mediator;
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
}
