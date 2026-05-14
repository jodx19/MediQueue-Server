// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Infrastructure\Persistence\Repositories\AppointmentRepository.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediQueue.Domain.Common;
using MediQueue.Domain.Entities;
using MediQueue.Domain.Interfaces;
using MediQueue.Infrastructure.Persistence.Context;

namespace MediQueue.Infrastructure.Persistence.Repositories;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly ClinicDbContext _context;

    public AppointmentRepository(ClinicDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets an appointment by ID.
    /// NOTE: Appointment has no navigation properties — only FK Guids.
    /// Callers that need Patient/Doctor info should load them separately.
    /// </summary>
    public async Task<Appointment?> GetByIdAsync(Guid id)
    {
        return await _context.Appointments
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    /// <summary>
    /// Returns available time slots for a doctor on a given date, excluding already-booked ones.
    /// Respects the doctor's WorkingShifts (stored as JSON) and slot duration.
    /// </summary>
    public async Task<List<TimeSlotDto>> GetAvailableSlotsAsync(Guid doctorId, DateTime date)
    {
        var doctor = await _context.Doctors
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == doctorId);

        if (doctor == null) return [];

        var shift = doctor.WorkingShifts.FirstOrDefault(s => s.DayOfWeek == date.DayOfWeek);
        if (shift == null) return [];

        // Load only the minimal data needed for conflict calculation
        var bookedSlots = await _context.Appointments
            .AsNoTracking()
            .Where(a =>
                a.DoctorId == doctorId &&
                a.ScheduledAt.Date == date.Date &&
                a.Status != Domain.Enums.AppointmentStatus.Cancelled)
            .Select(a => new { a.ScheduledAt, a.DurationMinutes })
            .ToListAsync();

        var slots = new List<TimeSlotDto>();

        foreach (var slotStart in shift.GenerateSlots())
        {
            var slotEnd = slotStart.AddMinutes(shift.SlotDurationMinutes);

            var hasConflict = bookedSlots.Any(b =>
            {
                var bStart = TimeOnly.FromDateTime(b.ScheduledAt);
                var bEnd = bStart.AddMinutes(b.DurationMinutes);
                return bStart < slotEnd && bEnd > slotStart;
            });

            if (!hasConflict)
            {
                slots.Add(new TimeSlotDto
                {
                    StartTime = slotStart,
                    EndTime = slotEnd
                });
            }
        }

        return slots;
    }

    /// <summary>
    /// Returns true if the doctor has any non-cancelled appointment that overlaps with
    /// [scheduledAt, scheduledAt + durationMinutes).
    /// Overlap formula: existing.ScheduledAt &lt; newEnd AND existingEnd &gt; newStart.
    /// </summary>
    public async Task<bool> HasConflictAsync(Guid doctorId, DateTime scheduledAt, int durationMinutes)
    {
        var newEnd = scheduledAt.AddMinutes(durationMinutes);

        return await _context.Appointments
            .AsNoTracking()
            .AnyAsync(a =>
                a.DoctorId == doctorId &&
                a.Status != Domain.Enums.AppointmentStatus.Cancelled &&
                a.ScheduledAt < newEnd &&
                a.ScheduledAt.AddMinutes(a.DurationMinutes) > scheduledAt);
    }

    /// <summary>Returns all appointments for a doctor on a specific date, ordered by time.</summary>
    public async Task<List<Appointment>> GetDoctorScheduleAsync(Guid doctorId, DateTime date)
    {
        return await _context.Appointments
            .AsNoTracking()
            .Include(a => a.Patient)
            .Where(a => a.DoctorId == doctorId && a.ScheduledAt.Date == date.Date)
            .OrderBy(a => a.ScheduledAt)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<List<Appointment>> GetByDateRangeAsync(
        DateTime fromUtc,
        DateTime toUtc,
        Guid? doctorId,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Appointments
            .AsNoTracking()
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Where(a => a.ScheduledAt >= fromUtc && a.ScheduledAt < toUtc);

        if (doctorId.HasValue)
            query = query.Where(a => a.DoctorId == doctorId.Value);

        return await query
            .OrderBy(a => a.ScheduledAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>Returns paginated appointment history for a patient.</summary>
    public async Task<PagedResult<Appointment>> GetPatientHistoryAsync(Guid patientId, int page, int size)
    {
        var query = _context.Appointments
            .AsNoTracking()
            .Where(a => a.PatientId == patientId)
            .OrderByDescending(a => a.ScheduledAt);

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        return new PagedResult<Appointment>(items, total, page, size);
    }

    public async Task AddAsync(Appointment appointment)
    {
        await _context.Appointments.AddAsync(appointment);
    }

    public async Task UpdateAsync(Appointment appointment)
    {
        _context.Appointments.Update(appointment);
        await Task.CompletedTask;
    }

    public async Task<int> CountByDateAsync(DateTime date)
    {
        return await _context.Appointments
            .AsNoTracking()
            .CountAsync(a => a.ScheduledAt.Date == date.Date);
    }

    public async Task<List<Appointment>> GetMissedAppointmentsAsync(DateTime threshold)
    {
        return await _context.Appointments
            .Where(a => a.Status == Domain.Enums.AppointmentStatus.Scheduled || a.Status == Domain.Enums.AppointmentStatus.Confirmed)
            .Where(a => a.ScheduledAt < threshold)
            .ToListAsync();
    }
}
