namespace MediQueue.Domain.Enums;

public enum TenantPlan
{
    Basic      = 1,  // 1 doctor, 100 patients/month
    Pro        = 2,  // 5 doctors, unlimited patients
    Enterprise = 3   // unlimited + white-label + AI
}
