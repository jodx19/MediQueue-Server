// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Infrastructure\Persistence\Repositories\PatientRepository.cs
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediQueue.Domain.Common;
using MediQueue.Domain.Entities;
using MediQueue.Domain.Interfaces;
using MediQueue.Infrastructure.Persistence.Context;

namespace MediQueue.Infrastructure.Persistence.Repositories;

public class PatientRepository : IPatientRepository
{
    private readonly ClinicDbContext _context;

    public PatientRepository(ClinicDbContext context)
    {
        _context = context;
    }

    public async Task<Patient?> GetByIdAsync(Guid id)
    {
        return await _context.Patients
            .AsNoTracking()
            .Include(p => p.Allergies)
            .Include(p => p.ChronicConditions)
            .Include(p => p.CurrentMedications)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Patient?> GetByMRNAsync(string medicalRecordNumber)
    {
        // IgnoreQueryFilters bypasses soft-delete only; tenant scope is always enforced.
        return await _context.Patients
            .IgnoreQueryFilters()
            .Where(p => p.TenantId == _context.CurrentTenantId && !p.IsDeleted)
            .AsNoTracking()
            .Include(p => p.Allergies)
            .Include(p => p.ChronicConditions)
            .Include(p => p.CurrentMedications)
            .FirstOrDefaultAsync(p => p.MedicalRecordNumber == medicalRecordNumber);
    }

    public async Task<Patient?> GetByNationalIdAsync(string nationalId)
    {
        return await _context.Patients
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.NationalId == nationalId);
    }

    public async Task<PagedResult<Patient>> SearchAsync(string term, int page, int size)
    {
        var query = _context.Patients.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(term))
        {
            var searchLower = term.ToLower();
            query = query.Where(p =>
                p.PersonName.FirstName.ToLower().Contains(searchLower) ||
                p.PersonName.LastName.ToLower().Contains(searchLower) ||
                (p.PersonName.MiddleName != null && p.PersonName.MiddleName.ToLower().Contains(searchLower)) ||
                p.MedicalRecordNumber.ToLower().Contains(searchLower) ||
                p.NationalId.ToLower().Contains(searchLower) ||
                p.ContactInfo.Phone.Contains(searchLower));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(p => p.PersonName.FirstName)
            .ThenBy(p => p.PersonName.LastName)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        return new PagedResult<Patient>(items, totalCount, page, size);
    }

    public async Task AddAsync(Patient patient)
    {
        await _context.Patients.AddAsync(patient);
    }

    public async Task UpdateAsync(Patient patient)
    {
        _context.Patients.Update(patient);
        await Task.CompletedTask;
    }

    public async Task<int> CountAsync()
    {
        return await _context.Patients.CountAsync();
    }


    /// <summary>
    /// Performs a soft delete by loading the entity and calling SoftDelete().
    /// The actual UPDATE is committed by the caller via SaveChangesAsync.
    /// </summary>
    public async Task SoftDeleteAsync(Guid id)
    {
        // Step 1: normal filtered lookup (covers the common case —
        // record exists, not deleted, belongs to current tenant)
        var patient = await _context.Patients
            .FirstOrDefaultAsync(p => p.Id == id);

        // Step 2: if not found, check within current tenant only,
        // bypassing soft-delete filter (edge case: already deleted)
        if (patient is null)
        {
            patient = await _context.Patients
                .IgnoreQueryFilters()
                .Where(p => p.TenantId == _context.CurrentTenantId)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        if (patient is not null)
        {
            patient.SoftDelete();
            _context.Patients.Update(patient);
        }
    }
}
