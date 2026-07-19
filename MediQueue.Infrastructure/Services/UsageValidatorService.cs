using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediQueue.Application.Interfaces;
using MediQueue.Domain.Enums;
using MediQueue.Infrastructure.Persistence.Context;

namespace MediQueue.Infrastructure.Services;

public class UsageValidatorService : IUsageValidatorService
{
    private readonly ClinicDbContext _dbContext;

    public UsageValidatorService(ClinicDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> IsQuotaAvailableAsync(Guid tenantId, QuotaType quotaType)
    {
        var tenant = await _dbContext.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == tenantId);

        if (tenant == null)
            return false;

        return quotaType switch
        {
            QuotaType.Patients => await _dbContext.Patients.CountAsync(p => p.TenantId == tenantId) < tenant.MaxPatients,
            QuotaType.Doctors => await _dbContext.Doctors.CountAsync(d => d.TenantId == tenantId) < tenant.MaxDoctors,
            QuotaType.Appointments => await CheckAppointmentsQuotaAsync(tenantId, tenant.MaxAppointmentsPerMonth),
            _ => false
        };
    }

    private async Task<bool> CheckAppointmentsQuotaAsync(Guid tenantId, int maxAppointments)
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        
        var count = await _dbContext.Appointments
            .CountAsync(a => a.TenantId == tenantId && a.ScheduledAt >= startOfMonth);

        return count < maxAppointments;
    }
}
