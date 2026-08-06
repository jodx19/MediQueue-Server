// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Domain\Interfaces\IAppointmentRepository.cs
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediQueue.Domain.Entities;
using MediQueue.Domain.Common;

namespace MediQueue.Domain.Interfaces;

public class TimeSlotDto
{
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
}

public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(Guid id);
    Task<List<TimeSlotDto>> GetAvailableSlotsAsync(Guid doctorId, DateTime date);
    Task<bool> HasConflictAsync(Guid doctorId, DateTime scheduledAt, int durationMinutes);
    Task<List<Appointment>> GetDoctorScheduleAsync(Guid doctorId, DateTime date);
    /// <summary>Appointments with <c>ScheduledAt</c> in <c>[fromUtc, toUtc)</c>, optional doctor filter.</summary>
    Task<List<Appointment>> GetByDateRangeAsync(
        DateTime fromUtc,
        DateTime toUtc,
        Guid? doctorId,
        CancellationToken cancellationToken = default);
    Task<PagedResult<Appointment>> GetPatientHistoryAsync(Guid patientId, int page, int size);
    Task<List<Appointment>> GetActiveAppointmentsByPatientIdAsync(Guid patientId);
    Task AddAsync(Appointment appointment);
    Task UpdateAsync(Appointment appointment);
    Task<int> CountByDateAsync(DateTime date);
    Task<List<Appointment>> GetMissedAppointmentsAsync(DateTime threshold);
}
