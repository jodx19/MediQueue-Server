using System;
using MediQueue.Domain.Common;
using MediQueue.Domain.Enums;

namespace MediQueue.Domain.Entities;

public class Tenant : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Subdomain { get; private set; } = string.Empty;
    public string AdminEmail { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public TenantPlan Plan { get; private set; } = TenantPlan.Basic;
    public DateTime TrialEndsAt { get; private set; }
    public DateTime? SubscriptionEndsAt { get; private set; }
    
    // Limits
    public int MaxPatients { get; private set; }
    public int MaxDoctors { get; private set; }
    public int MaxAppointmentsPerMonth { get; private set; }

    private Tenant() { }

    public static Tenant Create(
        string name,
        string subdomain,
        string adminEmail,
        TenantPlan plan = TenantPlan.Basic,
        int trialDays = 14)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(subdomain);

        var tenant = new Tenant
        {
            Name = name,
            Subdomain = subdomain.ToLowerInvariant().Trim(),
            AdminEmail = adminEmail,
            Plan = plan,
            TrialEndsAt = DateTime.UtcNow.AddDays(trialDays)
        };
        
        // Setup Plan Quotas
        switch (plan)
        {
            case TenantPlan.Basic:
                tenant.MaxDoctors = 1;
                tenant.MaxPatients = 100;
                tenant.MaxAppointmentsPerMonth = 500;
                break;
            case TenantPlan.Pro:
                tenant.MaxDoctors = 5;
                tenant.MaxPatients = int.MaxValue; // unlimited
                tenant.MaxAppointmentsPerMonth = int.MaxValue;
                break;
            case TenantPlan.Enterprise:
                tenant.MaxDoctors = int.MaxValue;
                tenant.MaxPatients = int.MaxValue;
                tenant.MaxAppointmentsPerMonth = int.MaxValue;
                break;
        }
        
        // Tenant owns itself
        tenant.TenantId = Guid.Empty;

        return tenant;
    }

    public void Activate() => IsActive = true;
    public void Suspend() => IsActive = false;

    public bool IsTrialActive()
        => TrialEndsAt > DateTime.UtcNow;

    public bool IsSubscriptionActive()
        => SubscriptionEndsAt.HasValue &&
           SubscriptionEndsAt.Value > DateTime.UtcNow;

    public bool CanAccess()
        => IsActive && (IsTrialActive() || IsSubscriptionActive());
}
