using System;
using System.Threading;
using System.Threading.Tasks;
using MediQueue.Domain.Enums;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.Tenants.Services;

public interface ITenantUsageService
{
    Task<bool> CanAddDoctorAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<bool> CanAddPatientAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<bool> CanAddAppointmentAsync(Guid tenantId, CancellationToken cancellationToken = default);
}

public class TenantUsageService : ITenantUsageService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantRepository _tenantRepository;

    public TenantUsageService(IUnitOfWork unitOfWork, ITenantRepository tenantRepository)
    {
        _unitOfWork = unitOfWork;
        _tenantRepository = tenantRepository;
    }

    public async Task<bool> CanAddDoctorAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var tenant = await _tenantRepository.GetByIdAsync(tenantId, cancellationToken);
        if (tenant == null || TenantPlanLimits.IsUnlimited(tenant.Plan)) return true;

        var limits = TenantPlanLimits.GetLimits(tenant.Plan);
        var currentCount = await _unitOfWork.Doctors.CountAsync();
        return currentCount < limits.MaxDoctors;
    }

    public async Task<bool> CanAddPatientAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var tenant = await _tenantRepository.GetByIdAsync(tenantId, cancellationToken);
        if (tenant == null || TenantPlanLimits.IsUnlimited(tenant.Plan)) return true;

        var limits = TenantPlanLimits.GetLimits(tenant.Plan);
        var currentCount = await _unitOfWork.Patients.CountAsync();
        return currentCount < limits.MaxPatients;
    }

    public async Task<bool> CanAddAppointmentAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var tenant = await _tenantRepository.GetByIdAsync(tenantId, cancellationToken);
        if (tenant == null || TenantPlanLimits.IsUnlimited(tenant.Plan)) return true;

        var limits = TenantPlanLimits.GetLimits(tenant.Plan);
        
        var now = DateTime.UtcNow;
        var firstDayOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var firstDayOfNextMonth = firstDayOfMonth.AddMonths(1);

        var appointmentsThisMonth = await _unitOfWork.Appointments.GetByDateRangeAsync(
            firstDayOfMonth, 
            firstDayOfNextMonth, 
            null, 
            cancellationToken);

        return appointmentsThisMonth.Count < limits.MaxAppointmentsPerMonth;
    }
}
