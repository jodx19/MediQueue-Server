// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Domain\Interfaces\IClinicalVisitRepository.cs
using System;
using System.Threading.Tasks;
using MediQueue.Domain.Entities;
using MediQueue.Domain.Common;

namespace MediQueue.Domain.Interfaces;

public interface IClinicalVisitRepository
{
    Task<ClinicalVisit?> GetByIdAsync(Guid id);
    Task<ClinicalVisit?> GetByAppointmentIdAsync(Guid appointmentId);
    Task<PagedResult<ClinicalVisit>> GetPatientHistoryAsync(Guid patientId, int page, int size);
    Task AddAsync(ClinicalVisit visit);
    Task UpdateAsync(ClinicalVisit visit);
}
