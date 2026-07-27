using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using MediQueue.Application.AuditLogs.Queries;

namespace MediQueue.API.Controllers;

[Authorize(Policy = "AdminOnly")]
[Route("api/audit-logs")]
public class AuditLogsController : BaseApiController
{
    private readonly IMediator _mediator;

    public AuditLogsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAuditLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] Guid? userId = null, [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
    {
        var query = new GetAuditLogsQuery 
        { 
            Page = page, 
            PageSize = pageSize,
            UserId = userId,
            From = from,
            To = to
        };

        var result = await _mediator.Send(query);
        return HandleResult(result);
    }
}
