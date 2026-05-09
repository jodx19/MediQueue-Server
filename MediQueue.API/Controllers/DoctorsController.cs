// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.API\Controllers\DoctorsController.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediQueue.Application.Common;
using MediQueue.Application.Doctors.Commands;
using MediQueue.Application.Doctors.Queries;
using MediQueue.Application.Doctors.DTOs;
using MediQueue.Domain.Enums;
using MediQueue.API.Models;

namespace MediQueue.API.Controllers;

/// <summary>Doctor management endpoints.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class DoctorsController : BaseApiController
{
    /// <summary>Create a new doctor profile.</summary>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<DoctorDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<DoctorDto>> Create(
        [FromBody] CreateDoctorCommand command,
        CancellationToken ct)
    {
        return HandleResult(await Sender.Send(command, ct));
    }

    /// <summary>Get a doctor by their unique ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<DoctorDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<DoctorDto>> GetById(Guid id, CancellationToken ct)
    {
        return HandleResult(await Sender.Send(new GetDoctorByIdQuery(id), ct));
    }

    /// <summary>Get all doctors, paginated.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<DoctorSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<DoctorSummaryDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        CancellationToken ct = default)
    {
        var result = await Sender.Send(new GetAllDoctorsQuery(page, size), ct);
        return Ok(result.Value);
    }

    /// <summary>Get doctors filtered by specialty (cached 10 minutes).</summary>
    [HttpGet("specialty/{specialty}")]
    [ProducesResponseType(typeof(DoctorDto[]), StatusCodes.Status200OK)]
    public async Task<ActionResult<DoctorDto[]>> GetBySpecialty(
        MedicalSpecialty specialty,
        CancellationToken ct)
    {
        var result = await Sender.Send(new GetDoctorsBySpecialtyQuery(specialty), ct);
        return Ok(result.Value);
    }

    /// <summary>Get a doctor's available time slots for a given date (cached 5 minutes).</summary>
    [HttpGet("{id:guid}/availability")]
    [ProducesResponseType(typeof(DoctorAvailabilityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DoctorAvailabilityDto>> GetAvailability(
        Guid id,
        [FromQuery] DateTime date,
        CancellationToken ct)
    {
        var result = await Sender.Send(new GetDoctorAvailabilityQuery(id, date), ct);
        if (!result.IsSuccess) return NotFound(result.Error);
        return Ok(result.Value);
    }

    /// <summary>Update a doctor's profile.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateDoctorCommand command,
        CancellationToken ct)
    {
        if (id != command.DoctorId) return BadRequest("Route ID must match command DoctorId.");
        var result = await Sender.Send(command, ct);
        if (!result.IsSuccess) return NotFound(result.Error);
        return NoContent();
    }

    /// <summary>Add a working shift to a doctor's schedule.</summary>
    [HttpPost("{id:guid}/shifts")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AddShift(
        Guid id,
        [FromBody] AddWorkingShiftCommand command,
        CancellationToken ct)
    {
        if (id != command.DoctorId) return BadRequest("Route ID must match command DoctorId.");
        var result = await Sender.Send(command, ct);
        if (!result.IsSuccess) return UnprocessableEntity(result.Error);
        return NoContent();
    }

    /// <summary>Remove a working shift from a doctor's schedule.</summary>
    [HttpDelete("{id:guid}/shifts/{dayOfWeek}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveShift(
        Guid id,
        DayOfWeek dayOfWeek,
        CancellationToken ct)
    {
        var result = await Sender.Send(new RemoveWorkingShiftCommand(id, dayOfWeek), ct);
        if (!result.IsSuccess) return NotFound(result.Error);
        return NoContent();
    }

    /// <summary>Mark a doctor as unavailable.</summary>
    [HttpPost("{id:guid}/unavailable")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetUnavailable(
        Guid id,
        [FromBody] SetDoctorUnavailableCommand command,
        CancellationToken ct)
    {
        if (id != command.DoctorId) return BadRequest("Route ID must match command DoctorId.");
        var result = await Sender.Send(command, ct);
        if (!result.IsSuccess) return NotFound(result.Error);
        return NoContent();
    }
}
