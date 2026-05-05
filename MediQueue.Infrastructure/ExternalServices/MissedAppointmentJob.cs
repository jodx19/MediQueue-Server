// Path: MediQueue.Infrastructure/ExternalServices/MissedAppointmentJob.cs
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediQueue.Domain.Enums;
using MediQueue.Domain.Interfaces;
using MediQueue.Infrastructure.Persistence.Context;

namespace MediQueue.Infrastructure.ExternalServices;

public class MissedAppointmentJob
{
    private readonly ClinicDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public MissedAppointmentJob(ClinicDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task ExecuteAsync()
    {
        var threshold = DateTime.UtcNow.AddMinutes(-30);
        
        var missedAppointments = await _context.Appointments
            .Where(a => a.Status == AppointmentStatus.Scheduled || a.Status == AppointmentStatus.Confirmed)
            .Where(a => a.ScheduledAt < threshold)
            .ToListAsync();

        foreach (var appointment in missedAppointments)
        {
            appointment.MarkNoShow();
        }

        if (missedAppointments.Any())
        {
            await _context.SaveChangesAsync();
        }
    }
}
