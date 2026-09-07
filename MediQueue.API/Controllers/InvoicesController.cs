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
using MediQueue.Application.Interfaces;
using MediQueue.Domain.Enums;
using AppCreatePaymentRequest = MediQueue.Application.Interfaces.CreatePaymentSessionRequest;

namespace MediQueue.API.Controllers;

/// <summary>Invoice and payment management endpoints.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class InvoicesController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IPaymentGatewayService _paymentGateway;

    public InvoicesController(ISender sender, IPaymentGatewayService paymentGateway)
    {
        _sender = sender;
        _paymentGateway = paymentGateway;
    }


    /// <summary>Paginated clinic-wide invoice list with optional status and date filters.</summary>
    [HttpGet]
    [Authorize(Policy = "AdminOrReceptionist")]
    [ProducesResponseType(typeof(PagedResult<InvoiceListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<InvoiceListItemDto>>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        CancellationToken ct = default)
    {
        var result = await _sender.Send(new GetClinicInvoicesQuery(status, from, to, page, pageSize), ct);
        if (!result.IsSuccess) return BadRequest(result.Error);
        return Ok(result.Value);
    }

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
    [Authorize(Policy = "AdminOrReceptionist")]
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
    [Authorize(Policy = "AdminOrReceptionist")]
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
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        CancellationToken ct)
    {
        var result = await _sender.Send(new GetRevenueReportQuery(DateOnly.FromDateTime(from), DateOnly.FromDateTime(to)), ct);
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
    [Authorize(Policy = "AdminOrReceptionist")]
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

    // ── Payment Gateway Endpoints ─────────────────────────────────────────────

    /// <summary>
    /// Create a hosted payment checkout session for an invoice.
    /// Returns a URL to redirect the patient to the payment provider's page.
    /// Currently backed by StubPaymentService — swap to Paymob/Fawry/Stripe in DependencyInjection.cs.
    /// </summary>
    [HttpPost("{id:guid}/create-payment-session")]
    [Authorize(Policy = "AdminOrReceptionist")]
    [ProducesResponseType(typeof(PaymentSessionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreatePaymentSession(
        Guid id,
        [FromBody] CheckoutSessionRequest request,
        CancellationToken ct)
    {
        // Load invoice details for amount
        var invoiceResult = await _sender.Send(new GetInvoiceByIdQuery(id), ct);
        if (!invoiceResult.IsSuccess) return NotFound(invoiceResult.Error);

        var invoice = invoiceResult.Value!;
        if (invoice.Status == InvoiceStatus.Paid)
            return UnprocessableEntity("Invoice is already paid.");

        // Map controller DTO → application DTO (avoid naming clash)
        var paymentRequest = new AppCreatePaymentRequest
        {
            InvoiceId = id,
            Amount = invoice.TotalAmount,
            Currency = request.Currency ?? "EGP",
            PatientName = request.PatientName,
            PatientEmail = request.PatientEmail,
            SuccessUrl = request.SuccessUrl,
            CancelUrl = request.CancelUrl,
        };

        var result = await _paymentGateway.CreateCheckoutSessionAsync(paymentRequest, ct);
        if (!result.IsSuccess)
            return UnprocessableEntity(result.ErrorMessage);

        return Ok(new PaymentSessionResponse
        {
            CheckoutUrl = result.CheckoutUrl!,
            SessionId = result.SessionId!,
            InvoiceId = id,
        });
    }

    /// <summary>
    /// Payment provider webhook — called by the payment gateway after successful payment.
    /// Verifies the transaction and marks the invoice as paid.
    /// </summary>
    [HttpPost("webhook")]
    [AllowAnonymous] // Webhook must be accessible without auth token
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> PaymentWebhook(
        [FromBody] PaymentWebhookPayload payload,
        CancellationToken ct)
    {
        var verification = await _paymentGateway.VerifyPaymentAsync(payload.TransactionReference, ct);
        if (!verification.IsPaid)
            return Ok(new { received = true, processed = false }); // Always return 200 to provider

        // Record the payment against the invoice
        var command = new RecordPaymentCommand
        {
            InvoiceId = payload.InvoiceId,
            Amount = verification.AmountPaid ?? payload.Amount,
            PaymentMethod = PaymentMethod.Online,
            ReferenceNumber = verification.TransactionId,
        };
        await _sender.Send(command, ct);

        return Ok(new { received = true, processed = true });
    }
}

// ── Request / Response DTOs (Controller level) ────────────────────────────────

/// <summary>Request body for creating a payment checkout session (controller-level DTO).</summary>
public class CheckoutSessionRequest
{
    public string? Currency { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string PatientEmail { get; set; } = string.Empty;
    public string SuccessUrl { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = string.Empty;
}

public class PaymentSessionResponse
{
    public string CheckoutUrl { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public Guid InvoiceId { get; set; }
}

public class PaymentWebhookPayload
{
    public Guid InvoiceId { get; set; }
    public string TransactionReference { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
