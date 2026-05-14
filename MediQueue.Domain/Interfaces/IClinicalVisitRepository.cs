// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Domain\Interfaces\IClinicalVisitRepository.cs
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediQueue.Domain.Entities;
using MediQueue.Domain.Common;

namespace MediQueue.Domain.Interfaces;

public interface IClinicalVisitRepository
{
    Task<ClinicalVisit?> GetByIdAsync(Guid id);
    Task<ClinicalVisit?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ClinicalVisit?> GetByAppointmentIdAsync(Guid appointmentId);
    Task<ClinicalVisit?> GetByAppointmentIdWithDetailsAsync(Guid appointmentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<Guid, Guid>> GetVisitIdsByAppointmentIdsAsync(
        IEnumerable<Guid> appointmentIds,
        CancellationToken cancellationToken = default);
    Task<PagedResult<ClinicalVisit>> GetPatientHistoryAsync(Guid patientId, int page, int size);
    Task AddAsync(ClinicalVisit visit);
    Task UpdateAsync(ClinicalVisit visit);
}
