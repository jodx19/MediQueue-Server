// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Domain\Enums\PaymentMethod.cs
namespace MediQueue.Domain.Enums;

/// <summary>
/// Represents the payment method used for a transaction.
/// </summary>
public enum PaymentMethod
{
    Cash = 1,
    CreditCard = 2,
    Insurance = 3,
    BankTransfer = 4,
    Installment = 5,
    /// <summary>Online payment via a payment gateway (Paymob, Fawry, Stripe, etc.).</summary>
    Online = 6
}
