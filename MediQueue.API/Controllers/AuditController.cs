using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediQueue.Application.Audit.Queries;
using MediQueue.Application.Common;
using MediQueue.Application.Common.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MediQueue.API.Controllers;

[ApiController]
[Route("api/audit")]
[Authorize(Policy = "AdminOnly")]
public sealed class AuditController : BaseApiController
{
    /// <summary>
    /// HIPAA/GDPR: Admin فقط
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AuditLogDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] DateTime? from       = null,
        [FromQuery] DateTime? to         = null,
        [FromQuery] string?   action     = null,
        [FromQuery] string?   entityName = null,
        [FromQuery] Guid?     userId     = null,
        [FromQuery] int       page       = 1,
        [FromQuery] int       pageSize   = 50,
        CancellationToken ct = default)
    {
        var result = await Sender.Send(
            new GetAuditLogsQuery(
                from, to, action, entityName,
                userId, page, pageSize),
            ct);

        return Ok(result);
    }

    /// <summary>
    /// مثال: GET /api/audit/entity/Patient/[patientId]
    /// </summary>
    [HttpGet("entity/{entityName}/{entityId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<AuditLogDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEntityAudit(
        string entityName,
        Guid   entityId,
        CancellationToken ct = default)
    {
        var result = await Sender.Send(
            new GetEntityAuditQuery(entityId, entityName),
            ct);

        return Ok(result);
    }
}
