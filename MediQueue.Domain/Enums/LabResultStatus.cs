// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Domain\Enums\LabResultStatus.cs
namespace MediQueue.Domain.Enums;

/// <summary>
/// Represents the status of a lab request or result.
/// </summary>
public enum LabResultStatus
{
    Pending = 1,
    InProgress = 2,
    Completed = 3,
    Abnormal = 4,
    Critical = 5
}
