// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Infrastructure\Persistence\Repositories\UserRepository.cs
using System;
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
        return await _context.Set<AppUser>()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email);
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
