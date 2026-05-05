// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Domain\Entities\Referral.cs
using System;
using MediQueue.Domain.Common;
using MediQueue.Domain.Enums;

namespace MediQueue.Domain.Entities;


/// <summary>
/// Represents a medical referral made during a clinical visit.
/// </summary>
public class Referral : BaseEntity
{
    public MedicalSpecialty ReferredToSpecialty { get; private set; }
    public string? ReferredToDoctorName { get; private set; }
    public string Reason { get; private set; }
    public ReferralUrgency Urgency { get; private set; }
    public string? Notes { get; private set; }

    private Referral() 
    { 
        // For EF Core
        Reason = null!;
    }

    internal Referral(MedicalSpecialty referredToSpecialty, string reason, ReferralUrgency urgency, string? referredToDoctorName = null, string? notes = null)
    {
        ReferredToSpecialty = referredToSpecialty;
        Reason = reason;
        Urgency = urgency;
        ReferredToDoctorName = referredToDoctorName;
        Notes = notes;
    }
}
