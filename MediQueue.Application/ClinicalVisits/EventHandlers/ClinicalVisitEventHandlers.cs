// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\ClinicalVisits\EventHandlers\ClinicalVisitEventHandlers.cs
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MediQueue.Application.Interfaces;
using MediQueue.Domain.Entities;
using MediQueue.Domain.Events;
using MediQueue.Domain.Interfaces;
using MediQueue.Domain.ValueObjects;

using MediQueue.Application.Common;

namespace MediQueue.Application.ClinicalVisits.EventHandlers;

public class ClinicalVisitCreatedEventHandler : INotificationHandler<DomainEventNotification<ClinicalVisitCreatedEvent>>
{
    private readonly IRealtimeService _realtimeService;

    public ClinicalVisitCreatedEventHandler(IRealtimeService realtimeService)
    {
        _realtimeService = realtimeService;
    }

    public async Task Handle(DomainEventNotification<ClinicalVisitCreatedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        await _realtimeService.BroadcastAsync("VisitStarted", new { 
            domainEvent.VisitId, 
            domainEvent.AppointmentId, 
            domainEvent.PatientId 
        });
    }
}

public class VisitFinalizedEventHandler : INotificationHandler<DomainEventNotification<VisitFinalizedEvent>>
{
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;

    public VisitFinalizedEventHandler(
        IEmailService emailService,
        IUnitOfWork unitOfWork)
    {
        _emailService = emailService;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DomainEventNotification<VisitFinalizedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        // 1. Update appointment status to Completed
        var visit = await _unitOfWork.ClinicalVisits.GetByIdAsync(domainEvent.VisitId);
        if (visit == null) return;

        var appointment = await _unitOfWork.Appointments.GetByIdAsync(visit.AppointmentId);
        if (appointment != null)
        {
            appointment.Complete();
            await _unitOfWork.Appointments.UpdateAsync(appointment);
        }

        // 2. Auto-create invoice
        if (visit.Procedures.Any())
        {
            var invoice = Invoice.Create(visit.PatientId, visit.AppointmentId, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)));
            
            foreach (var procedure in visit.Procedures)
            {
                invoice.AddItem($"Procedure: {procedure.MedicalCode.Description}", 1, procedure.Fee);
            }

            await _unitOfWork.Invoices.AddAsync(invoice);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        // 3. Send visit summary
        var patient = await _unitOfWork.Patients.GetByIdAsync(visit.PatientId);
        if (patient != null && !string.IsNullOrEmpty(patient.ContactInfo.Email))
        {
            _ = _emailService.SendVisitSummaryAsync(patient.ContactInfo.Email, "Your visit summary is ready.");
        }
    }
}

public class PrescriptionCreatedEventHandler : INotificationHandler<DomainEventNotification<PrescriptionCreatedEvent>>
{
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;

    public PrescriptionCreatedEventHandler(IEmailService emailService, IUnitOfWork unitOfWork)
    {
        _emailService = emailService;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DomainEventNotification<PrescriptionCreatedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        var visit = await _unitOfWork.ClinicalVisits.GetByIdAsync(domainEvent.VisitId);
        if (visit == null) return;

        var patient = await _unitOfWork.Patients.GetByIdAsync(visit.PatientId);
        
        if (patient != null && !string.IsNullOrEmpty(patient.ContactInfo.Email))
        {
            var prescriptionDetails = $"Prescription {domainEvent.PrescriptionId} created for your visit.";
            _ = _emailService.SendPrescriptionAsync(patient.ContactInfo.Email, prescriptionDetails);
        }
    }
}
