// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Domain\Enums\RequestStatus.cs
namespace MediQueue.Domain.Enums;

/// <summary>
/// Represents the status of a lab or imaging request.
/// </summary>
public enum RequestStatus
{
    Pending = 1,
    Ordered = 2,
    InProgress = 3,
    Completed = 4,
    Cancelled = 5
}
