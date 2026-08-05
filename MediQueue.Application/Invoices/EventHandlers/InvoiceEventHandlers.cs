using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MediQueue.Application.Interfaces;
using MediQueue.Domain.Events;
using MediQueue.Application.Common;

namespace MediQueue.Application.Invoices.EventHandlers;

public class InvoicePaidEventHandler : INotificationHandler<DomainEventNotification<InvoicePaidEvent>>
{
    private readonly IRealtimeService _realtimeService;

    public InvoicePaidEventHandler(IRealtimeService realtimeService)
    {
        _realtimeService = realtimeService;
    }

    public async Task Handle(DomainEventNotification<InvoicePaidEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        
        // Broadcast InvoicePaid event to the frontend (used by dashboard live revenue)
        await _realtimeService.BroadcastAsync("InvoicePaid", new { 
            domainEvent.InvoiceId, 
            domainEvent.PatientId, 
            domainEvent.OccurredAt 
        });
    }
}
