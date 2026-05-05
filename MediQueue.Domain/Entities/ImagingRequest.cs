// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Domain\Entities\ImagingRequest.cs
using System;
using MediQueue.Domain.Common;
using MediQueue.Domain.Enums;

namespace MediQueue.Domain.Entities;


/// <summary>
/// Represents a medical imaging request made during a clinical visit.
/// </summary>
public class ImagingRequest : BaseEntity
{
    public ImagingType ImagingType { get; private set; }
    public string BodyPart { get; private set; }
    public string? Instructions { get; private set; }
    public LabResultStatus Status { get; private set; }

    private ImagingRequest() 
    { 
        // For EF Core
        BodyPart = null!;
    }

    internal ImagingRequest(ImagingType imagingType, string bodyPart, string? instructions)
    {
        ImagingType = imagingType;
        BodyPart = bodyPart;
        Instructions = instructions;
        Status = LabResultStatus.Pending;
    }

    public void UpdateStatus(LabResultStatus status)
    {
        Status = status;
        SetUpdated();
    }
}
