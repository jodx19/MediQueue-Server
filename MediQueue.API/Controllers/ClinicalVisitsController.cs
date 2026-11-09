// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.API\Controllers\ClinicalVisitsController.cs
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediQueue.Application.Common;
using MediQueue.Application.ClinicalVisits.Commands;
using MediQueue.Application.ClinicalVisits.Queries;
using MediQueue.Application.ClinicalVisits.DTOs;
using System.Linq;

namespace MediQueue.API.Controllers;

/// <summary>Clinical visit (SOAP notes, vitals, diagnoses, procedures, prescriptions) endpoints.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class ClinicalVisitsController : ControllerBase
{
    private readonly ISender _sender;
    public ClinicalVisitsController(ISender sender) => _sender = sender;

    // ── Queries ──────────────────────────────────────────────────────────────

    /// <summary>Get clinical visits for currently authenticated patient.</summary>
    [HttpGet("self")]
    [Authorize(Policy = "PatientOnly")]
    [ProducesResponseType(typeof(PagedResult<ClinicalVisitSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ClinicalVisitSummaryDto>>> GetMine(
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        CancellationToken ct = default)
    {
        var patientClaim = User.FindFirst("PatientId")?.Value ?? User.FindFirst("patientId")?.Value;
        if (!Guid.TryParse(patientClaim, out var patientId)) return Forbid();
        var result = await _sender.Send(new GetPatientClinicalHistoryQuery(patientId, page, size), ct);
        return Ok(result.Value);
    }

    /// <summary>Get all prescriptions for currently authenticated patient.</summary>
    [HttpGet("self/prescriptions")]
    [Authorize(Policy = "PatientOnly")]
    [ProducesResponseType(typeof(PrescriptionDto[]), StatusCodes.Status200OK)]
    public async Task<ActionResult<PrescriptionDto[]>> GetMyPrescriptions(CancellationToken ct)
    {
        var patientClaim = User.FindFirst("PatientId")?.Value ?? User.FindFirst("patientId")?.Value;
        if (!Guid.TryParse(patientClaim, out var patientId)) return Forbid();
        var result = await _sender.Send(new GetPatientPrescriptionsQuery(patientId), ct);
        return Ok(result.Value);
    }

    /// <summary>Get a specific clinical visit detail for currently authenticated patient.</summary>
    [HttpGet("self/{id:guid}")]
    [Authorize(Policy = "PatientOnly")]
    [ProducesResponseType(typeof(ClinicalVisitDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClinicalVisitDetailDto>> GetMyVisitById(Guid id, CancellationToken ct)
    {
        var patientClaim = User.FindFirst("PatientId")?.Value ?? User.FindFirst("patientId")?.Value;
        if (!Guid.TryParse(patientClaim, out var patientId)) return Forbid();
        var result = await _sender.Send(new GetClinicalVisitByIdQuery(id), ct);
        if (!result.IsSuccess) return NotFound(result.Error);
        if (result.Value!.PatientId != patientId) return Forbid();
        return Ok(result.Value);
    }

    /// <summary>Get a clinical visit by its ID.</summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "PatientOrStaff")]
    [ProducesResponseType(typeof(ClinicalVisitDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClinicalVisitDetailDto>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new GetClinicalVisitByIdQuery(id), ct);
        if (!result.IsSuccess) return NotFound(result.Error);

        if (User.IsInRole("Patient"))
        {
            var patientClaim = User.FindFirst("PatientId")?.Value ?? User.FindFirst("patientId")?.Value;
            if (!Guid.TryParse(patientClaim, out var pid) || result.Value!.PatientId != pid)
                return Forbid();
        }

        return Ok(result.Value);
    }

    /// <summary>Get the clinical visit associated with a specific appointment.</summary>
    [HttpGet("appointment/{appointmentId:guid}")]
    [Authorize(Policy = "AdminOrDoctor")]
    [ProducesResponseType(typeof(ClinicalVisitDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClinicalVisitDetailDto>> GetByAppointment(Guid appointmentId, CancellationToken ct)
    {
        var result = await _sender.Send(new GetVisitByAppointmentQuery(appointmentId), ct);
        if (!result.IsSuccess) return NotFound(result.Error);
        return Ok(result.Value);
    }

    /// <summary>Get paginated clinical visit history for a patient.</summary>
    [HttpGet("patient/{patientId:guid}")]
    [Authorize(Policy = "PatientOrStaff")]
    [ProducesResponseType(typeof(PagedResult<ClinicalVisitSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ClinicalVisitSummaryDto>>> GetPatientHistory(
        Guid patientId,
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        CancellationToken ct = default)
    {
        if (User.IsInRole("Patient"))
        {
            var patientClaim = User.FindFirst("PatientId")?.Value ?? User.FindFirst("patientId")?.Value;
            if (!Guid.TryParse(patientClaim, out var pid) || pid != patientId)
                return Forbid();
        }

        var result = await _sender.Send(new GetPatientClinicalHistoryQuery(patientId, page, size), ct);
        return Ok(result.Value);
    }

    /// <summary>Get all prescriptions for a patient.</summary>
    [HttpGet("patient/{patientId:guid}/prescriptions")]
    [Authorize(Policy = "PatientOrStaff")]
    [ProducesResponseType(typeof(PrescriptionDto[]), StatusCodes.Status200OK)]
    public async Task<ActionResult<PrescriptionDto[]>> GetPatientPrescriptions(
        Guid patientId,
        CancellationToken ct)
    {
        if (User.IsInRole("Patient"))
        {
            var patientClaim = User.FindFirst("PatientId")?.Value ?? User.FindFirst("patientId")?.Value;
            if (!Guid.TryParse(patientClaim, out var pid) || pid != patientId)
                return Forbid();
        }

        var result = await _sender.Send(new GetPatientPrescriptionsQuery(patientId), ct);
        return Ok(result.Value);
    }

    // ── Commands ─────────────────────────────────────────────────────────────

    /// <summary>Create a new clinical visit linked to an in-progress appointment.</summary>
    [HttpPost]
    [Authorize(Policy = "DoctorOnly")]
    [ProducesResponseType(typeof(ClinicalVisitDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ClinicalVisitDto>> Create(
        [FromBody] CreateClinicalVisitCommand command,
        CancellationToken ct)
    {
        var result = await _sender.Send(command, ct);
        if (!result.IsSuccess) return UnprocessableEntity(result.Error);
        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    /// <summary>Update the SOAP note for a visit.</summary>
    [HttpPut("{id:guid}/soap")]
    [Authorize(Policy = "DoctorOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateSOAP(
        Guid id,
        [FromBody] UpdateSOAPNoteCommand command,
        CancellationToken ct)
    {
        if (id != command.VisitId) return BadRequest("Route ID must match command VisitId.");
        var result = await _sender.Send(command, ct);
        if (!result.IsSuccess) return UnprocessableEntity(result.Error);
        return NoContent();
    }

    /// <summary>Add a vital sign measurement to a visit.</summary>
    [HttpPost("{id:guid}/vital-signs")]
    [Authorize(Policy = "DoctorOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AddVitalSign(
        Guid id,
        [FromBody] AddVitalSignCommand command,
        CancellationToken ct)
    {
        if (id != command.VisitId) return BadRequest("Route ID must match command VisitId.");
        var result = await _sender.Send(command, ct);
        return result.IsSuccess ? NoContent() : UnprocessableEntity(result.Error);
    }

    /// <summary>Add a diagnosis (ICD-10) to a visit.</summary>
    [HttpPost("{id:guid}/diagnoses")]
    [Authorize(Policy = "DoctorOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AddDiagnosis(
        Guid id,
        [FromBody] AddDiagnosisCommand command,
        CancellationToken ct)
    {
        if (id != command.VisitId) return BadRequest("Route ID must match command VisitId.");
        var result = await _sender.Send(command, ct);
        return result.IsSuccess ? NoContent() : UnprocessableEntity(result.Error);
    }

    /// <summary>Add a medical procedure to a visit.</summary>
    [HttpPost("{id:guid}/procedures")]
    [Authorize(Policy = "DoctorOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AddProcedure(
        Guid id,
        [FromBody] AddProcedureCommand command,
        CancellationToken ct)
    {
        if (id != command.VisitId) return BadRequest("Route ID must match command VisitId.");
        var result = await _sender.Send(command, ct);
        return result.IsSuccess ? NoContent() : UnprocessableEntity(result.Error);
    }

    /// <summary>Add a lab test request to a visit.</summary>
    [HttpPost("{id:guid}/lab-requests")]
    [Authorize(Policy = "DoctorOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AddLabRequest(
        Guid id,
        [FromBody] AddLabRequestCommand command,
        CancellationToken ct)
    {
        if (id != command.VisitId) return BadRequest("Route ID must match command VisitId.");
        var result = await _sender.Send(command, ct);
        return result.IsSuccess ? NoContent() : UnprocessableEntity(result.Error);
    }

    /// <summary>Add an imaging request to a visit.</summary>
    [HttpPost("{id:guid}/imaging-requests")]
    [Authorize(Policy = "DoctorOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AddImagingRequest(
        Guid id,
        [FromBody] AddImagingRequestCommand command,
        CancellationToken ct)
    {
        if (id != command.VisitId) return BadRequest("Route ID must match command VisitId.");
        var result = await _sender.Send(command, ct);
        return result.IsSuccess ? NoContent() : UnprocessableEntity(result.Error);
    }

    /// <summary>Add a specialist referral to a visit.</summary>
    [HttpPost("{id:guid}/referrals")]
    [Authorize(Policy = "DoctorOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AddReferral(
        Guid id,
        [FromBody] AddReferralCommand command,
        CancellationToken ct)
    {
        if (id != command.VisitId) return BadRequest("Route ID must match command VisitId.");
        var result = await _sender.Send(command, ct);
        return result.IsSuccess ? NoContent() : UnprocessableEntity(result.Error);
    }

    /// <summary>Create a prescription for a visit.</summary>
    [HttpPost("{id:guid}/prescriptions")]
    [Authorize(Policy = "DoctorOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreatePrescription(
        Guid id,
        [FromBody] CreatePrescriptionCommand command,
        CancellationToken ct)
    {
        if (id != command.VisitId) return BadRequest("Route ID must match command VisitId.");
        var result = await _sender.Send(command, ct);
        return result.IsSuccess ? NoContent() : UnprocessableEntity(result.Error);
    }

    /// <summary>Complete a lab request with results.</summary>
    [HttpPut("{id:guid}/lab-requests/{labRequestId:guid}/complete")]
    [Authorize(Policy = "DoctorOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompleteLabRequest(
        Guid id,
        Guid labRequestId,
        [FromBody] CompleteLabRequestCommand command,
        CancellationToken ct)
    {
        if (id != command.VisitId) return BadRequest("Route visitId must match command VisitId.");
        command.LabRequestId = labRequestId;
        var result = await _sender.Send(command, ct);
        return result.IsSuccess ? NoContent() : NotFound(result.Error);
    }

    /// <summary>
    /// Update lab request details (test name, instructions, status).
    /// For attaching results and marking as completed/abnormal/critical, use the /complete endpoint.
    /// </summary>
    [HttpPut("{id:guid}/lab-requests/{labRequestId:guid}")]
    [Authorize(Policy = "AdminOrDoctor")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateLabRequest(
        Guid id,
        Guid labRequestId,
        [FromBody] UpdateLabRequestCommand command,
        CancellationToken ct)
    {
        if (id != command.VisitId) return BadRequest("Route visitId must match command VisitId.");
        command.LabRequestId = labRequestId;
        var result = await _sender.Send(command, ct);
        return result.IsSuccess ? NoContent() : NotFound(result.Error);
    }

    /// <summary>
    /// Finalize a clinical visit. Validates all SOAP sections are complete and
    /// at least one diagnosis exists. Auto-triggers invoice creation via event handler.
    /// </summary>
    [HttpPost("{id:guid}/finalize")]
    [Authorize(Policy = "DoctorOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Finalize(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new FinalizeClinicalVisitCommand(id), ct);
        return result.IsSuccess ? NoContent() : UnprocessableEntity(result.Error);
    }
}
