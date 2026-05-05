// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Domain\Entities\LabRequest.cs
using System;
using MediQueue.Domain.Common;
using MediQueue.Domain.Enums;

namespace MediQueue.Domain.Entities;

/// <summary>
/// Represents a laboratory test request made during a clinical visit.
/// </summary>
public class LabRequest : BaseEntity
{
    public string TestName { get; private set; }
    public string? Instructions { get; private set; }
    public DateTime RequestedAt { get; private set; }
    public LabResultStatus Status { get; private set; }
    public string? ResultValue { get; private set; }
    public string? ResultNotes { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private LabRequest() 
    { 
        // For EF Core
        TestName = null!;
    }

    internal LabRequest(string testName, string? instructions)
    {
        TestName = testName;
        Instructions = instructions;
        RequestedAt = DateTime.UtcNow;
        Status = LabResultStatus.Pending;
    }

    public void UpdateResult(string resultValue, string? resultNotes, LabResultStatus status)
    {
        ResultValue = resultValue;
        ResultNotes = resultNotes;
        Status = status;

        if (status == LabResultStatus.Completed || status == LabResultStatus.Abnormal || status == LabResultStatus.Critical)
        {
            CompletedAt = DateTime.UtcNow;
        }
        
        SetUpdated();
    }
}
