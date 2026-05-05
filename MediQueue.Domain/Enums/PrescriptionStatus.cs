// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Domain\Enums\PrescriptionStatus.cs
namespace MediQueue.Domain.Enums;

/// <summary>
/// Represents the status of a prescription.
/// </summary>
public enum PrescriptionStatus
{
    Active = 1,
    Completed = 2,
    Cancelled = 3,
    OnHold = 4
}
