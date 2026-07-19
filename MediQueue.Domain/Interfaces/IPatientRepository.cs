// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Domain\Interfaces\IPatientRepository.cs
using System;
using System.Threading.Tasks;
using MediQueue.Domain.Entities;
using MediQueue.Domain.Common;

namespace MediQueue.Domain.Interfaces;

public interface IPatientRepository
{
    Task<Patient?> GetByIdAsync(Guid id);
    Task<Patient?> GetByNationalIdAsync(string nationalId);
    Task<Patient?> GetByMRNAsync(string mrn);
    Task<Patient?> GetByMRNInCurrentTenantAsync(string mrn);
    Task<PagedResult<Patient>> SearchAsync(string term, int page, int size);
    Task AddAsync(Patient patient);
    Task UpdateAsync(Patient patient);
    Task<int> CountAsync();
    Task SoftDeleteAsync(Guid id);
}
