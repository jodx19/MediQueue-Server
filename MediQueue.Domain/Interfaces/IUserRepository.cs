// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Domain\Interfaces\IUserRepository.cs
using System;
using System.Threading.Tasks;
using MediQueue.Domain.Entities;

namespace MediQueue.Domain.Interfaces;

public interface IUserRepository
{
    Task<AppUser?> GetByIdAsync(Guid id);
    Task<AppUser?> GetByUsernameAsync(string username);
    Task<AppUser?> GetByEmailAsync(string email);
    Task AddAsync(AppUser user);
    Task UpdateAsync(AppUser user);
}
