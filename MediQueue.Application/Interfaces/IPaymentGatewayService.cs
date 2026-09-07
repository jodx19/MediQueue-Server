using System;
using System.Threading;
using System.Threading.Tasks;

namespace MediQueue.Application.Interfaces;

/// <summary>
/// Payment gateway abstraction.
/// Current implementation is a stub — wire to a real provider (Paymob, Fawry, Stripe, etc.)
/// by replacing <see cref="MediQueue.Infrastructure.ExternalServices.StubPaymentService"/> 
/// with the real implementation and registering it in DependencyInjection.cs.
/// </summary>
public interface IPaymentGatewayService
{
    /// <summary>
    /// Creates a hosted checkout session for the given invoice.
    /// Returns a URL the patient navigates to in order to pay.
    /// </summary>
    Task<PaymentSessionResult> CreateCheckoutSessionAsync(
        CreatePaymentSessionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies a completed payment using the provider's transaction reference.
    /// Called from the webhook endpoint after the provider confirms payment.
    /// </summary>
    Task<PaymentVerificationResult> VerifyPaymentAsync(
        string transactionReference,
        CancellationToken cancellationToken = default);
}

/// <summary>Request to create a payment checkout session.</summary>
public class CreatePaymentSessionRequest
{
    /// <summary>MediQueue internal invoice ID.</summary>
    public Guid InvoiceId { get; init; }

    /// <summary>Invoice amount in the smallest currency unit (e.g., piastres for EGP).</summary>
    public decimal Amount { get; init; }

    /// <summary>ISO 4217 currency code (e.g., "EGP", "SAR", "USD").</summary>
    public string Currency { get; init; } = "EGP";

    /// <summary>Patient full name for the payment page.</summary>
    public string PatientName { get; init; } = string.Empty;

    /// <summary>Patient email for receipt.</summary>
    public string PatientEmail { get; init; } = string.Empty;

    /// <summary>URL to redirect to after successful payment.</summary>
    public string SuccessUrl { get; init; } = string.Empty;

    /// <summary>URL to redirect to if patient cancels payment.</summary>
    public string CancelUrl { get; init; } = string.Empty;
}

public class PaymentSessionResult
{
    public bool IsSuccess { get; init; }
    public string? CheckoutUrl { get; init; }
    public string? SessionId { get; init; }
    public string? ErrorMessage { get; init; }

    public static PaymentSessionResult Success(string checkoutUrl, string sessionId)
        => new() { IsSuccess = true, CheckoutUrl = checkoutUrl, SessionId = sessionId };

    public static PaymentSessionResult Failure(string error)
        => new() { IsSuccess = false, ErrorMessage = error };
}

public class PaymentVerificationResult
{
    public bool IsPaid { get; init; }
    public string? TransactionId { get; init; }
    public decimal? AmountPaid { get; init; }
    public string? ErrorMessage { get; init; }

    public static PaymentVerificationResult Paid(string transactionId, decimal amount)
        => new() { IsPaid = true, TransactionId = transactionId, AmountPaid = amount };

    public static PaymentVerificationResult NotPaid(string? reason = null)
        => new() { IsPaid = false, ErrorMessage = reason };
}
