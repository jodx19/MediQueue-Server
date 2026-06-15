// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Infrastructure\Persistence\Repositories\UserRepository.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediQueue.Domain.Entities;
using MediQueue.Domain.Interfaces;
using MediQueue.Infrastructure.Persistence.Context;

namespace MediQueue.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ClinicDbContext _context;

    public UserRepository(ClinicDbContext context)
    {
        _context = context;
    }

    public async Task<AppUser?> GetByIdAsync(Guid id)
    {
        return await _context.Set<AppUser>()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<AppUser?> GetByUsernameAsync(string username)
    {
        return await _context.Set<AppUser>()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<AppUser?> GetByEmailAsync(string email)
    {
        // Pre-auth lookup: must find the user regardless of the
        // host-resolved tenant context (user may belong to a
        // different tenant than the subdomain they're browsing).
        // Manually preserve soft-delete filtering since
        // IgnoreQueryFilters() bypasses BOTH TenantId and IsDeleted.
        return await _context.Set<AppUser>()
            .IgnoreQueryFilters()
            .Where(u => !u.IsDeleted)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<AppUser?> GetByRefreshTokenAsync(string refreshToken)
    {
        return await _context.Set<AppUser>()
            .IgnoreQueryFilters()
            .Where(u => !u.IsDeleted)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
    }

    public async Task<AppUser?> GetByPatientIdAsync(Guid patientId)
    {
        return await _context.Set<AppUser>()
            .IgnoreQueryFilters()
            .Where(u => !u.IsDeleted)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.PatientId == patientId);
    }

    public async Task<AppUser?> GetByDoctorIdAsync(Guid doctorId)
    {
        return await _context.Set<AppUser>()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.DoctorId == doctorId);
    }

    public async Task<IEnumerable<AppUser>> GetAllAsync()
    {
        return await _context.Set<AppUser>()
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddAsync(AppUser user)
    {
        await _context.Set<AppUser>().AddAsync(user);
    }

    public async Task UpdateAsync(AppUser user)
    {
        _context.Set<AppUser>().Update(user);
        await Task.CompletedTask;
    }
}
