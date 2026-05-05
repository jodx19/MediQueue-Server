// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.API\Controllers\DashboardController.cs
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using MediQueue.Application.Dashboard.DTOs;
using MediQueue.Application.Dashboard.Queries;

namespace MediQueue.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("stats")]
    public async Task<ActionResult<ClinicStatsDto>> GetStats()
    {
        var result = await _mediator.Send(new GetClinicStatsQuery());
        if (!result.IsSuccess)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpGet("revenue-report")]
    public async Task<ActionResult<RevenueReportDto>> GetRevenueReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var result = await _mediator.Send(new GetRevenueReportQuery(startDate, endDate));
        if (!result.IsSuccess)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }
}
