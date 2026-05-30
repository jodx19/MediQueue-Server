using System.Collections.Generic;
using System.Threading.Tasks;
using MediQueue.Domain.Entities;

namespace MediQueue.Domain.Interfaces;

public interface IUserRepository
{
    Task<AppUser?> GetByIdAsync(Guid id);
    Task<AppUser?> GetByUsernameAsync(string username);
    Task<AppUser?> GetByEmailAsync(string email);
    Task<AppUser?> GetByRefreshTokenAsync(string refreshToken);
    Task<AppUser?> GetByPatientIdAsync(Guid patientId);
    Task<AppUser?> GetByDoctorIdAsync(Guid doctorId);
    Task<IEnumerable<AppUser>> GetAllAsync();
    Task AddAsync(AppUser user);
    Task UpdateAsync(AppUser user);
}
