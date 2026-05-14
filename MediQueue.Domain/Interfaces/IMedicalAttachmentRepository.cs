// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Domain\Interfaces\IMedicalAttachmentRepository.cs
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediQueue.Domain.Entities;

namespace MediQueue.Domain.Interfaces;

public interface IMedicalAttachmentRepository
{
    Task<MedicalAttachment?> GetByIdAsync(Guid id);
    Task<List<MedicalAttachment>> GetByPatientIdAsync(Guid patientId);
    Task<List<MedicalAttachment>> GetByClinicalVisitIdAsync(Guid clinicalVisitId, CancellationToken cancellationToken = default);
    Task AddAsync(MedicalAttachment attachment);
    Task DeleteAsync(MedicalAttachment attachment);
}
