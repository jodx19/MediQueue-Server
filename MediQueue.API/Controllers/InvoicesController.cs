// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.API\Controllers\InvoicesController.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediQueue.Application.Common;
using MediQueue.Application.Invoices.Commands;
using MediQueue.Application.Invoices.Queries;
using MediQueue.Application.Invoices.DTOs;

namespace MediQueue.API.Controllers;

/// <summary>Invoice and payment management endpoints.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class InvoicesController : ControllerBase
{
    private readonly ISender _sender;
    public InvoicesController(ISender sender) => _sender = sender;

    /// <summary>Create a new invoice (auto-generates INV-YYYYMMDD-XXXX number).</summary>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<InvoiceDto>> Create(
        [FromBody] CreateInvoiceCommand command,
        CancellationToken ct)
    {
        var result = await _sender.Send(command, ct);
        if (!result.IsSuccess) return UnprocessableEntity(result.Error);
        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    /// <summary>Get an invoice by its unique ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InvoiceDto>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new GetInvoiceByIdQuery(id), ct);
        if (!result.IsSuccess) return NotFound(result.Error);
        return Ok(result.Value);
    }

    /// <summary>Get paginated invoices for a patient with optional status filter.</summary>
    [HttpGet("patient/{patientId:guid}")]
    [ProducesResponseType(typeof(PagedResult<InvoiceDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<InvoiceDto>>> GetByPatient(
        Guid patientId,
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        CancellationToken ct = default)
    {
        var result = await _sender.Send(new GetPatientInvoicesQuery(patientId, page, size), ct);
        return Ok(result.Value);
    }

    /// <summary>Get a revenue report grouped by date and payment method.</summary>
    [HttpGet("revenue-report")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(RevenueReportDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<RevenueReportDto>> GetRevenueReport(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        CancellationToken ct)
    {
        var result = await _sender.Send(new GetRevenueReportQuery(from, to), ct);
        return Ok(result.Value);
    }

    /// <summary>Add a line item to a draft invoice.</summary>
    [HttpPost("{id:guid}/items")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AddItem(
        Guid id,
        [FromBody] AddInvoiceItemCommand command,
        CancellationToken ct)
    {
        if (id != command.InvoiceId) return BadRequest("Route ID must match command InvoiceId.");
        var result = await _sender.Send(command, ct);
        return result.IsSuccess ? NoContent() : UnprocessableEntity(result.Error);
    }

    /// <summary>Apply a discount to a draft invoice.</summary>
    [HttpPost("{id:guid}/discount")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ApplyDiscount(
        Guid id,
        [FromBody] ApplyDiscountCommand command,
        CancellationToken ct)
    {
        if (id != command.InvoiceId) return BadRequest("Route ID must match command InvoiceId.");
        var result = await _sender.Send(command, ct);
        return result.IsSuccess ? NoContent() : UnprocessableEntity(result.Error);
    }

    /// <summary>Record a payment against an invoice. Raises InvoicePaidEvent when fully paid.</summary>
    [HttpPost("{id:guid}/payments")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RecordPayment(
        Guid id,
        [FromBody] RecordPaymentCommand command,
        CancellationToken ct)
    {
        if (id != command.InvoiceId) return BadRequest("Route ID must match command InvoiceId.");
        var result = await _sender.Send(command, ct);
        return result.IsSuccess ? NoContent() : UnprocessableEntity(result.Error);
    }

    /// <summary>Cancel an invoice (only allowed when not fully paid).</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new CancelInvoiceCommand(id), ct);
        return result.IsSuccess ? NoContent() : UnprocessableEntity(result.Error);
    }
}
