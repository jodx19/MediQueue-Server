// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Domain\Enums\InvoiceStatus.cs
namespace MediQueue.Domain.Enums;

/// <summary>
/// Represents the status of an invoice.
/// </summary>
public enum InvoiceStatus
{
    Draft = 1,
    Issued = 2,
    PartiallyPaid = 3,
    Paid = 4,
    Overdue = 5,
    Cancelled = 6
}
