using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediQueue.Application.Interfaces;
using MediQueue.Application.Settings.Dtos;
using MediQueue.Infrastructure.Persistence.Context;
using MediQueue.Infrastructure.Persistence.Entities;

namespace MediQueue.Infrastructure.Repositories;

public class SettingsRepository : ISettingsRepository
{
    private readonly ClinicDbContext _context;
    private readonly ITenantContext _tenantContext;

    public SettingsRepository(
        ClinicDbContext context,
        ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<ClinicSettingsDto> GetSettingsAsync(CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;
        var settings = await _context.ClinicSettings
            .Where(s => s.TenantId == tenantId)
            .FirstOrDefaultAsync(cancellationToken);

        if (settings == null)
        {
            settings = new ClinicSettings
            {
                ClinicName = "MediQueue Dental Clinic",
                ClinicPhone = "01000000000",
                ClinicEmail = "info@mediqueue.com",
                Currency = "EGP",
                TimeZone = "Egypt Standard Time",
                AllowOnlineBooking = true,
                TenantId = tenantId
            };
            _context.ClinicSettings.Add(settings);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return MapToDto(settings);
    }

    public async Task<ClinicSettingsDto> UpdateSettingsAsync(ClinicSettingsDto dto, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;
        var settings = await _context.ClinicSettings
            .Where(s => s.TenantId == tenantId)
            .FirstOrDefaultAsync(cancellationToken);

        if (settings == null)
        {
            settings = new ClinicSettings
            {
                ClinicName = "MediQueue Dental Clinic",
                ClinicPhone = "01000000000",
                ClinicEmail = "info@mediqueue.com",
                Currency = "EGP",
                TimeZone = "Egypt Standard Time",
                AllowOnlineBooking = true,
                TenantId = tenantId
            };
            _context.ClinicSettings.Add(settings);
            await _context.SaveChangesAsync(cancellationToken);
        }

        settings.ClinicName = dto.ClinicName;
        settings.ClinicPhone = dto.ClinicPhone;
        settings.ClinicEmail = dto.ClinicEmail;
        settings.ClinicAddress = dto.ClinicAddress;
        if (TimeOnly.TryParse(dto.WorkStartTime, out var startTime)) settings.WorkStartTime = startTime;
        if (TimeOnly.TryParse(dto.WorkEndTime, out var endTime)) settings.WorkEndTime = endTime;
        settings.AppointmentDurationMinutes = dto.AppointmentDurationMinutes;
        settings.Currency = dto.Currency;
        settings.TimeZone = dto.TimeZone;
        settings.AllowOnlineBooking = dto.AllowOnlineBooking;
        settings.RequireDepositForBooking = dto.RequireDepositForBooking;
        settings.DepositAmount = dto.DepositAmount;

        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(settings);
    }

    /// <inheritdoc />
    /// <remarks>
    /// Intentionally does NOT call <c>SaveChangesAsync</c>. The caller owns the
    /// UnitOfWork transaction (tenant creation) and will persist this seed row
    /// atomically together with the <c>Tenant</c> and the admin <c>AppUser</c>.
    /// The <see cref="ClinicDbContext"/> is registered as Scoped, so the same
    /// DbContext instance is shared with <c>UnitOfWork</c> within the request.
    /// </remarks>
    public Task SeedForTenantAsync(Guid tenantId, string clinicName, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var settings = new ClinicSettings
        {
            ClinicName = string.IsNullOrWhiteSpace(clinicName) ? "MediQueue Dental Clinic" : clinicName,
            ClinicPhone = string.Empty,
            ClinicEmail = string.Empty,
            ClinicAddress = string.Empty,
            LogoUrl = string.Empty,
            WorkStartTime = new TimeOnly(8, 0),
            WorkEndTime = new TimeOnly(20, 0),
            AppointmentDurationMinutes = 30,
            Currency = "EGP",
            TimeZone = "Egypt Standard Time",
            AllowOnlineBooking = true,
            RequireDepositForBooking = false,
            DepositAmount = 0m,
            CreatedAt = now,
            UpdatedAt = now,
            TenantId = tenantId
        };

        _context.ClinicSettings.Add(settings);
        return Task.CompletedTask;
    }

    private static ClinicSettingsDto MapToDto(ClinicSettings entity)
    {
        return new ClinicSettingsDto(
            entity.Id,
            entity.ClinicName,
            entity.ClinicPhone,
            entity.ClinicEmail,
            entity.ClinicAddress,
            entity.LogoUrl,
            entity.WorkStartTime.ToString("HH:mm"),
            entity.WorkEndTime.ToString("HH:mm"),
            entity.AppointmentDurationMinutes,
            entity.Currency,
            entity.TimeZone,
            entity.AllowOnlineBooking,
            entity.RequireDepositForBooking,
            entity.DepositAmount
        );
    }
}
