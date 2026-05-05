// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Infrastructure\Persistence\Repositories\DoctorRepository.cs
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediQueue.Domain.Entities;
using MediQueue.Domain.Enums;
using MediQueue.Domain.Interfaces;
using MediQueue.Domain.Common;
using MediQueue.Infrastructure.Persistence.Context;

namespace MediQueue.Infrastructure.Persistence.Repositories;

public class DoctorRepository : IDoctorRepository
{
    private readonly ClinicDbContext _context;

    public DoctorRepository(ClinicDbContext context)
    {
        _context = context;
    }

    public async Task<Doctor?> GetByIdAsync(Guid id)
    {
        return await _context.Doctors
            .AsNoTracking()
            .Include(d => d.Qualifications)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<PagedResult<Doctor>> GetAllAsync(int pageNumber, int pageSize)
    {
        var query = _context.Doctors.AsNoTracking().AsQueryable();
        
        var totalCount = await query.CountAsync();
        
        var items = await query
            .OrderBy(d => d.PersonName.FirstName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Doctor>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<List<Doctor>> GetBySpecialtyAsync(MedicalSpecialty specialty)
    {
        return await _context.Doctors
            .AsNoTracking()
            .Where(d => d.Specialty == specialty)
            .OrderBy(d => d.PersonName.FirstName)
            .ToListAsync();
    }

    public async Task AddAsync(Doctor doctor)
    {
        await _context.Doctors.AddAsync(doctor);
    }

    public async Task UpdateAsync(Doctor doctor)
    {
        _context.Doctors.Update(doctor);
        await Task.CompletedTask;
    }

    public async Task<int> CountAsync()
    {
        return await _context.Doctors.CountAsync();
    }
}
