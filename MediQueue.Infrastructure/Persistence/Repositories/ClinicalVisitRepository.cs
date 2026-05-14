using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediQueue.Domain.Common;
using MediQueue.Domain.Entities;
using MediQueue.Domain.Interfaces;
using MediQueue.Infrastructure.Persistence.Context;

namespace MediQueue.Infrastructure.Persistence.Repositories;

public class ClinicalVisitRepository : IClinicalVisitRepository
{
    private readonly ClinicDbContext _context;

    public ClinicalVisitRepository(ClinicDbContext context)
    {
        _context = context;
    }

    public async Task<ClinicalVisit?> GetByIdAsync(Guid id)
    {
        return await _context.ClinicalVisits
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<ClinicalVisit?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ClinicalVisits
            .AsNoTracking()
            .AsSplitQuery()
            .Include(v => v.Patient).ThenInclude(p => p.Allergies)
            .Include(v => v.Patient).ThenInclude(p => p.ChronicConditions)
            .Include(v => v.Doctor)
            .Include(v => v.Appointment)
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
    }

    public async Task<ClinicalVisit?> GetByAppointmentIdAsync(Guid appointmentId)
    {
        return await _context.ClinicalVisits
            .FirstOrDefaultAsync(v => v.AppointmentId == appointmentId);
    }

    public async Task<ClinicalVisit?> GetByAppointmentIdWithDetailsAsync(
        Guid appointmentId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ClinicalVisits
            .AsNoTracking()
            .AsSplitQuery()
            .Include(v => v.Patient).ThenInclude(p => p.Allergies)
            .Include(v => v.Patient).ThenInclude(p => p.ChronicConditions)
            .Include(v => v.Doctor)
            .Include(v => v.Appointment)
            .FirstOrDefaultAsync(v => v.AppointmentId == appointmentId, cancellationToken);
    }

    public async Task<IReadOnlyDictionary<Guid, Guid>> GetVisitIdsByAppointmentIdsAsync(
        IEnumerable<Guid> appointmentIds,
        CancellationToken cancellationToken = default)
    {
        var idList = appointmentIds.Distinct().ToList();
        if (idList.Count == 0)
            return new Dictionary<Guid, Guid>();

        return await _context.ClinicalVisits
            .AsNoTracking()
            .Where(v => idList.Contains(v.AppointmentId))
            .ToDictionaryAsync(v => v.AppointmentId, v => v.Id, cancellationToken);
    }

    public async Task<PagedResult<ClinicalVisit>> GetPatientHistoryAsync(Guid patientId, int page, int size)
    {
        var query = _context.ClinicalVisits
            .Where(v => v.PatientId == patientId)
            .OrderByDescending(v => v.VisitDate);

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        return new PagedResult<ClinicalVisit>(items, total, page, size);
    }

    public async Task AddAsync(ClinicalVisit visit)
    {
        await _context.ClinicalVisits.AddAsync(visit);
    }

    public async Task UpdateAsync(ClinicalVisit visit)
    {
        _context.ClinicalVisits.Update(visit);
        await Task.CompletedTask;
    }
}
