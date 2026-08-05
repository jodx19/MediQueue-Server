// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.API\Controllers\ReportsController.cs
using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediQueue.Application.Reports.Queries;

namespace MediQueue.API.Controllers;

/// <summary>
/// Provides clinic analytics and summary reports.
/// Accessible to Admins and Doctors only.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Doctor")]
[Produces("application/json")]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReportsController(IMediator mediator)
        => _mediator = mediator;

    /// <summary>Gets daily revenue data for the given date range.</summary>
    [HttpGet("revenue")]
    public async Task<ActionResult<ReportsResponse>> GetRevenue(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var result = await _mediator.Send(new GetRevenueReportQuery(startDate, endDate));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    /// <summary>Gets appointment count grouped by status for the given date range.</summary>
    [HttpGet("appointments")]
    public async Task<ActionResult<ReportsResponse>> GetAppointments(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var result = await _mediator.Send(new GetAppointmentsReportQuery(startDate, endDate));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    /// <summary>Gets patient totals and gender breakdown.</summary>
    [HttpGet("patients")]
    public async Task<ActionResult<ReportsResponse>> GetPatients()
    {
        var result = await _mediator.Send(new GetPatientsReportQuery());
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    /// <summary>Gets doctor totals grouped by specialty.</summary>
    [HttpGet("doctors")]
    public async Task<ActionResult<ReportsResponse>> GetDoctors()
    {
        var result = await _mediator.Send(new GetDoctorsReportQuery());
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}
