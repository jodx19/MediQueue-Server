// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Infrastructure\Persistence\Repositories\MedicalAttachmentRepository.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediQueue.Domain.Entities;
using MediQueue.Domain.Interfaces;
using MediQueue.Infrastructure.Persistence.Context;

namespace MediQueue.Infrastructure.Persistence.Repositories;

public class MedicalAttachmentRepository : IMedicalAttachmentRepository
{
    private readonly ClinicDbContext _context;

    public MedicalAttachmentRepository(ClinicDbContext context)
    {
        _context = context;
    }

    public async Task<MedicalAttachment?> GetByIdAsync(Guid id)
    {
        return await _context.Set<MedicalAttachment>()
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<List<MedicalAttachment>> GetByPatientIdAsync(Guid patientId)
    {
        return await _context.Set<MedicalAttachment>()
            .AsNoTracking()
            .Where(a => a.PatientId == patientId)
            .OrderByDescending(a => a.UploadedAt)
            .ToListAsync();
    }

    public async Task<List<MedicalAttachment>> GetByClinicalVisitIdAsync(
        Guid clinicalVisitId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<MedicalAttachment>()
            .AsNoTracking()
            .Where(a => a.ClinicalVisitId == clinicalVisitId)
            .OrderByDescending(a => a.UploadedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(MedicalAttachment attachment)
    {
        await _context.Set<MedicalAttachment>().AddAsync(attachment);
    }

    public async Task DeleteAsync(MedicalAttachment attachment)
    {
        _context.Set<MedicalAttachment>().Remove(attachment);
        await Task.CompletedTask;
    }
}
