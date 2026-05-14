// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.API\Controllers\AppointmentsController.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediQueue.Application.Common;
using MediQueue.Application.Appointments.Commands;
using MediQueue.Application.Appointments.Queries;
using System.Collections.Generic;
using MediQueue.Application.Appointments.DTOs;

namespace MediQueue.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class AppointmentsController : ControllerBase
{
    private readonly ISender _sender;
    public AppointmentsController(ISender sender) => _sender = sender;

    /// <summary>Book a new appointment.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<AppointmentDto>> Book([FromBody] BookAppointmentCommand command, CancellationToken ct)
    {
        var result = await _sender.Send(command, ct);
        if (!result.IsSuccess) return UnprocessableEntity(result.Error);
        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    /// <summary>Get appointment by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AppointmentDto>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new GetAppointmentByIdQuery(id), ct);
        if (!result.IsSuccess) return NotFound(result.Error);
        return Ok(result.Value);
    }

    /// <summary>Get today's appointments (cached 1 min).</summary>
    [HttpGet("today")]
    [ProducesResponseType(typeof(List<AppointmentListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AppointmentListItemDto>>> GetToday([FromQuery] Guid? doctorId, CancellationToken ct)
    {
        var result = await _sender.Send(new GetTodaysAppointmentsQuery(doctorId), ct);
        return Ok(result.Value);
    }

    /// <summary>Get upcoming appointments.</summary>
    [HttpGet("upcoming")]
    [ProducesResponseType(typeof(List<AppointmentListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AppointmentListItemDto>>> GetUpcoming(
        [FromQuery] int days = 7,
        [FromQuery] Guid? doctorId = null,
        CancellationToken ct = default)
    {
        var result = await _sender.Send(new GetUpcomingAppointmentsQuery(days, doctorId), ct);
        return Ok(result.Value);
    }

    /// <summary>Appointments in a date range (calendar / schedule).</summary>
    [HttpGet("schedule")]
    [ProducesResponseType(typeof(List<AppointmentScheduleItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AppointmentScheduleItemDto>>> GetSchedule(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] Guid? doctorId = null,
        CancellationToken ct = default)
    {
        var result = await _sender.Send(new GetAppointmentsInRangeQuery(from, to, doctorId), ct);
        if (!result.IsSuccess) return BadRequest(result.Error);
        return Ok(result.Value);
    }

    /// <summary>Get a doctor's schedule for a date.</summary>
    [HttpGet("doctor/{doctorId:guid}/schedule")]
    [ProducesResponseType(typeof(List<AppointmentScheduleItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AppointmentScheduleItemDto>>> GetDoctorSchedule(Guid doctorId, [FromQuery] DateTime date, CancellationToken ct)
    {
        var result = await _sender.Send(new GetDoctorScheduleQuery(doctorId, date), ct);
        return Ok(result.Value);
    }

    /// <summary>Get paginated appointment history for a patient.</summary>
    [HttpGet("patient/{patientId:guid}")]
    public async Task<ActionResult<PagedResult<AppointmentDto>>> GetPatientHistory(
        Guid patientId, [FromQuery] int page = 1, [FromQuery] int size = 20, CancellationToken ct = default)
        => Ok((await _sender.Send(new GetPatientAppointmentsQuery(patientId, page, size), ct)).Value);

    /// <summary>Confirm a scheduled appointment.</summary>
    [HttpPut("{id:guid}/confirm")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<AppointmentDto>> Confirm(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new ConfirmAppointmentCommand(id), ct);
        return result.IsSuccess ? Ok(result.Value) : UnprocessableEntity(result.Error);
    }

    /// <summary>Check in the patient.</summary>
    [HttpPut("{id:guid}/check-in")]
    public async Task<ActionResult<AppointmentDto>> CheckIn(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new CheckInAppointmentCommand(id), ct);
        return result.IsSuccess ? Ok(result.Value) : UnprocessableEntity(result.Error);
    }

    /// <summary>Start the appointment (doctor begins).</summary>
    [HttpPut("{id:guid}/start")]
    [Authorize(Policy = "DoctorOnly")]
    public async Task<ActionResult<AppointmentDto>> Start(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new StartAppointmentCommand(id), ct);
        return result.IsSuccess ? Ok(result.Value) : UnprocessableEntity(result.Error);
    }

    /// <summary>Complete the appointment.</summary>
    [HttpPut("{id:guid}/complete")]
    [Authorize(Policy = "DoctorOnly")]
    public async Task<ActionResult<AppointmentDto>> Complete(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new CompleteAppointmentCommand(id), ct);
        return result.IsSuccess ? Ok(result.Value) : UnprocessableEntity(result.Error);
    }

    /// <summary>Cancel the appointment.</summary>
    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelAppointmentCommand command, CancellationToken ct)
    {
        if (id != command.AppointmentId) return BadRequest("ID mismatch.");
        var result = await _sender.Send(command, ct);
        return result.IsSuccess ? NoContent() : UnprocessableEntity(result.Error);
    }

    /// <summary>Reschedule the appointment.</summary>
    [HttpPost("{id:guid}/reschedule")]
    public async Task<IActionResult> Reschedule(Guid id, [FromBody] RescheduleAppointmentCommand command, CancellationToken ct)
    {
        if (id != command.AppointmentId) return BadRequest("ID mismatch.");
        var result = await _sender.Send(command, ct);
        return result.IsSuccess ? NoContent() : UnprocessableEntity(result.Error);
    }

    /// <summary>Mark patient as no-show.</summary>
    [HttpPut("{id:guid}/no-show")]
    public async Task<ActionResult<AppointmentDto>> MarkNoShow(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new MarkNoShowCommand(id), ct);
        return result.IsSuccess ? Ok(result.Value) : UnprocessableEntity(result.Error);
    }
}
