using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Domain.Events;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.Patients.EventHandlers;

public class PatientDeactivatedEventHandler : INotificationHandler<DomainEventNotification<PatientDeactivatedEvent>>
{
    private readonly IUnitOfWork _unitOfWork;

    public PatientDeactivatedEventHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DomainEventNotification<PatientDeactivatedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        
        // Find all active appointments for this patient
        var activeAppointments = await _unitOfWork.Appointments.GetActiveAppointmentsByPatientIdAsync(domainEvent.PatientId);

        foreach (var appointment in activeAppointments)
        {
            appointment.Cancel("Patient record deactivated");
            await _unitOfWork.Appointments.UpdateAsync(appointment);
        }

        // Save changes to persist cancellations
        if (activeAppointments.Count > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
