using System;
using System.Threading;
using System.Threading.Tasks;
using MediQueue.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace MediQueue.Infrastructure.ExternalServices;

/// <summary>
/// Stub payment gateway — simulates a successful payment flow without calling any real provider.
/// 
/// ┌──────────────────────────────────────────────────────────────────┐
/// │  TO INTEGRATE A REAL PROVIDER (e.g., Paymob, Fawry, Stripe):    │
/// │  1. Create a new service: PaymobPaymentService.cs                │
/// │  2. Implement IPaymentGatewayService                             │
/// │  3. In DependencyInjection.cs replace:                           │
/// │       services.AddSingleton<IPaymentGatewayService,              │
/// │           StubPaymentService>();                                  │
/// │     with:                                                         │
/// │       services.Configure<PaymobOptions>(...)                      │
/// │       services.AddHttpClient<IPaymentGatewayService,             │
/// │           PaymobPaymentService>();                                │
/// │                                                                  │
/// │  Paymob docs:  https://docs.paymob.com/docs/accept-standard-pk  │
/// │  Fawry docs:   https://developer.fawrystaging.com/               │
/// │  Stripe docs:  https://stripe.com/docs/api/checkout/sessions     │
/// └──────────────────────────────────────────────────────────────────┘
/// </summary>
public class StubPaymentService : IPaymentGatewayService
{
    private readonly ILogger<StubPaymentService> _logger;

    public StubPaymentService(ILogger<StubPaymentService> logger)
    {
        _logger = logger;
    }

    public Task<PaymentSessionResult> CreateCheckoutSessionAsync(
        CreatePaymentSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogWarning(
            "[Payment Stub] CreateCheckoutSession called for Invoice {InvoiceId} — Amount: {Amount} {Currency}. " +
            "No real payment gateway is configured. Replace StubPaymentService with a real provider.",
            request.InvoiceId, request.Amount, request.Currency);

        // Return a fake session that redirects back to the success URL immediately
        // This allows the UI flow to be tested end-to-end without a real gateway.
        var fakeSessionId = $"stub_session_{Guid.NewGuid():N}";
        var fakeCheckoutUrl = $"{request.SuccessUrl}?session_id={fakeSessionId}&invoice_id={request.InvoiceId}&stub=true";

        return Task.FromResult(PaymentSessionResult.Success(fakeCheckoutUrl, fakeSessionId));
    }

    public Task<PaymentVerificationResult> VerifyPaymentAsync(
        string transactionReference,
        CancellationToken cancellationToken = default)
    {
        _logger.LogWarning(
            "[Payment Stub] VerifyPayment called for ref '{Ref}'. " +
            "Returning auto-approved result — no real gateway.",
            transactionReference);

        // Always return paid for stub — in production, verify with the provider's API
        return Task.FromResult(PaymentVerificationResult.Paid(
            transactionId: transactionReference,
            amount: 0m // Amount unknown in stub mode
        ));
    }
}
