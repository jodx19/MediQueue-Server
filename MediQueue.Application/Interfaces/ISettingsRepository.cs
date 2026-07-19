using System;
using System.Threading;
using System.Threading.Tasks;
using MediQueue.Application.Settings.Dtos;

namespace MediQueue.Application.Interfaces;

public interface ISettingsRepository
{
    Task<ClinicSettingsDto> GetSettingsAsync(CancellationToken cancellationToken);
    Task<ClinicSettingsDto> UpdateSettingsAsync(ClinicSettingsDto dto, CancellationToken cancellationToken);

    /// <summary>
    /// Seeds default <see cref="ClinicSettings"/> row for a freshly-provisioned tenant.
    /// Must be called within the same UnitOfWork transaction as the tenant creation
    /// so the new tenant is never "bricked" on its first request.
    /// </summary>
    Task SeedForTenantAsync(Guid tenantId, string clinicName, CancellationToken cancellationToken);
}
