// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.API\Controllers\PatientsController.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediQueue.Application.Common;
using MediQueue.Application.Patients.Commands;
using MediQueue.Application.Patients.Queries;
using MediQueue.Application.Patients.DTOs;

namespace MediQueue.API.Controllers;

/// <summary>Patient management endpoints.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class PatientsController : BaseApiController
{
    /// <summary>Get all patients (paginated).</summary>
    [HttpGet]
    [Authorize(Policy = "StaffOnly")]
    [ProducesResponseType(typeof(PagedResult<PatientSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<PatientSummaryDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        CancellationToken ct = default)
    {
        var result = await Sender.Send(new SearchPatientsQuery(string.Empty, page, size), ct);
        return Ok(result.Value);
    }

    /// <summary>Register a new patient.</summary>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(PatientDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PatientDto>> Register(
        [FromBody] RegisterPatientCommand command,
        CancellationToken ct)
    {
        var tenantContext = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<MediQueue.Application.Interfaces.ITenantContext>(HttpContext.RequestServices);
        var usageService = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<MediQueue.Application.Tenants.Services.ITenantUsageService>(HttpContext.RequestServices);
        
        if (!await usageService.CanAddPatientAsync(tenantContext.TenantId, ct))
        {
            return StatusCode(402, "Tenant plan limit reached for patients. Please upgrade your plan.");
        }

        return HandleResult(await Sender.Send(command, ct));
    }

    /// <summary>Self-register a new patient (public access).</summary>
    [HttpPost("self-register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PatientDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PatientDto>> SelfRegister(
        [FromBody] SelfRegisterPatientCommand command,
        CancellationToken ct)
    {
        var tenantContext = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<MediQueue.Application.Interfaces.ITenantContext>(HttpContext.RequestServices);
        var usageService = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<MediQueue.Application.Tenants.Services.ITenantUsageService>(HttpContext.RequestServices);
        
        if (!await usageService.CanAddPatientAsync(tenantContext.TenantId, ct))
        {
            return StatusCode(402, "Tenant plan limit reached for patients. Please upgrade your plan.");
        }

        return HandleResult(await Sender.Send(command, ct));
    }

    /// <summary>Get a patient by their unique ID.</summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "StaffOnly")]
    [ProducesResponseType(typeof(PatientDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PatientDetailDto>> GetById(Guid id, CancellationToken ct)
    {
        var result = await Sender.Send(new GetPatientByIdQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    /// <summary>Get a patient by their Medical Record Number (MRN).</summary>
    [HttpGet("mrn/{mrn}")]
    [Authorize(Policy = "StaffOnly")]
    [ProducesResponseType(typeof(PatientDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PatientDetailDto>> GetByMRN(string mrn, CancellationToken ct)
    {
        var result = await Sender.Send(new GetPatientByMRNQuery(mrn), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    /// <summary>Search patients by name, MRN, or National ID (paginated).</summary>
    [HttpGet("search")]
    [Authorize(Policy = "StaffOnly")]
    [ProducesResponseType(typeof(PagedResult<PatientSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<PatientSummaryDto>>> Search(
        [FromQuery] string? term,
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        CancellationToken ct = default)
    {
        var result = await Sender.Send(new SearchPatientsQuery(term ?? string.Empty, page, size), ct);
        return Ok(result.Value);
    }

    /// <summary>Get full medical history for a patient.</summary>
    [HttpGet("{id:guid}/medical-history")]
    [Authorize(Policy = "AdminOrDoctor")]
    [ProducesResponseType(typeof(PatientMedicalHistoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PatientMedicalHistoryDto>> GetMedicalHistory(Guid id, CancellationToken ct)
    {
        var result = await Sender.Send(new GetPatientMedicalHistoryQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    /// <summary>Update contact information for a patient.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdatePatientCommand command,
        CancellationToken ct)
    {
        if (id != command.PatientId) return BadRequest("Route ID must match command PatientId.");
        var result = await Sender.Send(command, ct);
        return result.IsSuccess ? NoContent() : NotFound(result.Error);
    }

    /// <summary>Add an allergy to a patient's record.</summary>
    [HttpPost("{id:guid}/allergies")]
    [Authorize(Policy = "AdminOrDoctor")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddAllergy(
        Guid id,
        [FromBody] AddAllergyCommand command,
        CancellationToken ct)
    {
        if (id != command.PatientId) return BadRequest("Route ID must match command PatientId.");
        var result = await Sender.Send(command, ct);
        return result.IsSuccess ? NoContent() : NotFound(result.Error);
    }

    /// <summary>Remove an allergy from a patient's record.</summary>
    [HttpDelete("{id:guid}/allergies/{allergyId:guid}")]
    [Authorize(Policy = "AdminOrDoctor")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveAllergy(Guid id, Guid allergyId, CancellationToken ct)
    {
        var result = await Sender.Send(new RemoveAllergyCommand(id, allergyId), ct);
        return result.IsSuccess ? NoContent() : NotFound(result.Error);
    }

    /// <summary>Add a chronic condition to a patient's record.</summary>
    [HttpPost("{id:guid}/chronic-conditions")]
    [Authorize(Policy = "AdminOrDoctor")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddChronicCondition(
        Guid id,
        [FromBody] AddChronicConditionCommand command,
        CancellationToken ct)
    {
        if (id != command.PatientId) return BadRequest("Route ID must match command PatientId.");
        var result = await Sender.Send(command, ct);
        return result.IsSuccess ? NoContent() : NotFound(result.Error);
    }

    /// <summary>Remove a chronic condition from a patient's record.</summary>
    [HttpDelete("{id:guid}/chronic-conditions/{conditionId:guid}")]
    [Authorize(Policy = "AdminOrDoctor")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveChronicCondition(Guid id, Guid conditionId, CancellationToken ct)
    {
        var result = await Sender.Send(new RemoveChronicConditionCommand(id, conditionId), ct);
        return result.IsSuccess ? NoContent() : NotFound(result.Error);
    }

    /// <summary>Deactivate (soft-delete) a patient record.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        var result = await Sender.Send(new DeactivatePatientCommand(id), ct);
        return result.IsSuccess ? NoContent() : NotFound(result.Error);
    }
}
