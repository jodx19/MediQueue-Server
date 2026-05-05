// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Domain\Interfaces\IDoctorRepository.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediQueue.Domain.Entities;
using MediQueue.Domain.Enums;
using MediQueue.Domain.Common;

namespace MediQueue.Domain.Interfaces;

public interface IDoctorRepository
{
    Task<Doctor?> GetByIdAsync(Guid id);
    Task<List<Doctor>> GetBySpecialtyAsync(MedicalSpecialty specialty);
    Task<PagedResult<Doctor>> GetAllAsync(int page, int size);
    Task AddAsync(Doctor doctor);
    Task UpdateAsync(Doctor doctor);
    Task<int> CountAsync();
}
