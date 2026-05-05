// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Infrastructure\Persistence\Repositories\ClinicalVisitRepository.cs
using System;
using System.Linq;
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

    /// <summary>
    /// Gets a clinical visit by ID.
    /// NOTE: ClinicalVisit has no navigation property for Appointment — only AppointmentId (FK Guid).
    /// </summary>
    public async Task<ClinicalVisit?> GetByIdAsync(Guid id)
    {
        return await _context.ClinicalVisits
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<ClinicalVisit?> GetByAppointmentIdAsync(Guid appointmentId)
    {
        return await _context.ClinicalVisits
            .FirstOrDefaultAsync(v => v.AppointmentId == appointmentId);
    }

    /// <summary>
    /// Returns paginated clinical visit history for a patient, ordered by most recent first.
    /// Only summary-level data is loaded (no owned collections) for performance.
    /// </summary>
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
