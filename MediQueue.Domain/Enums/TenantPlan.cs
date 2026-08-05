namespace MediQueue.Domain.Enums;

public enum TenantPlan
{
    Basic      = 1,  // 1 doctor, 100 patients/month
    Pro        = 2,  // 5 doctors, unlimited patients
    Enterprise = 3   // unlimited + white-label + AI
}

/// <summary>
/// Per-plan usage quotas enforced by TenantUsageService.
/// </summary>
public static class TenantPlanLimits
{
    /// <returns>(MaxDoctors, MaxPatients, MaxAppointmentsPerMonth)</returns>
    public static (int MaxDoctors, int MaxPatients, int MaxAppointmentsPerMonth) GetLimits(TenantPlan plan)
        => plan switch
        {
            TenantPlan.Basic      => (1, 100, 200),
            TenantPlan.Pro        => (5, 500, 1000),
            TenantPlan.Enterprise => (int.MaxValue, int.MaxValue, int.MaxValue),
            _                     => (0, 0, 0)
        };

    public static bool IsUnlimited(TenantPlan plan) => plan == TenantPlan.Enterprise;
}
