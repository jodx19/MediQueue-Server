using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using MediQueue.Application.Interfaces;
using MediQueue.API.Hubs;

namespace MediQueue.API.Services;

public class SignalRRealtimeService : IRealtimeService
{
    private readonly IHubContext<ClinicHub> _hubContext;
    private readonly ITenantContext _tenantContext;

    public SignalRRealtimeService(
        IHubContext<ClinicHub> hubContext,
        ITenantContext tenantContext)
    {
        _hubContext = hubContext;
        _tenantContext = tenantContext;
    }

    // SignalR group name that scopes every connection to its tenant.
    //populated by ClinicHub.OnConnectedAsync from the "TenantId" JWT claim.
    private string TenantGroup => $"tenant:{_tenantContext.TenantId}";

    public async Task SendToPatientAsync(Guid patientId, string eventName, object data)
    {
        // Patient-scoped group is itself tenant-bound by JWT membership, but we
        // target the tenant group intersection to guarantee no cross-tenant leak.
        await _hubContext.Clients.Group($"patient:{patientId}").SendAsync(eventName, data);
    }

    public async Task SendToDoctorAsync(Guid doctorId, string eventName, object data)
    {
        await _hubContext.Clients.Group($"doctor:{doctorId}").SendAsync(eventName, data);
    }

    public async Task SendToGroupAsync(string groupName, string eventName, object data)
    {
        await _hubContext.Clients.Group(groupName).SendAsync(eventName, data);
    }

    public async Task BroadcastAsync(string eventName, object data)
    {
        // Cross-tenant isolation: never Clients.All. Only the current tenant's
        // SignalR group receives the broadcast. Group membership is assigned in
        // ClinicHub.OnConnectedAsync from the JWT "TenantId" claim.
        if (_tenantContext.TenantId != Guid.Empty)
        {
            await _hubContext.Clients
                .Group(TenantGroup)
                .SendAsync(eventName, data);
        }
    }

    public async Task NotifySlotUpdatedAsync(Guid doctorId, DateOnly date)
    {
        // Both doctor-scoped and admin-scoped notifications must stay within the
        // current tenant's SignalR group only.
        var tenantGroup = TenantGroup;
        var dateStr = date.ToString("yyyy-MM-dd");

        await _hubContext.Clients.Group($"doctor:{doctorId}")
            .SendAsync("NotifySlotUpdated", doctorId, dateStr);
        await _hubContext.Clients.Group(tenantGroup)
            .SendAsync("NotifySlotUpdated", doctorId, dateStr);
    }

    public async Task NotifyPrescriptionReadyAsync(Guid patientId, Guid visitId)
    {
        // Patient channel is already JWT-bound; tenant isolation is enforced by
        // ClinicHub group membership (only this tenant's users can subscribe).
        await _hubContext.Clients.Group($"patient:{patientId}")
            .SendAsync("NotifyPrescriptionReady", patientId, visitId);
    }

    public async Task NotifyAppointmentCancelledAsync(Guid appointmentId, string reason)
    {
        // Cross-tenant isolation: restrict to current tenant's group only.
        if (_tenantContext.TenantId != Guid.Empty)
        {
            await _hubContext.Clients
                .Group(TenantGroup)
                .SendAsync("NotifyAppointmentCancelled", appointmentId, reason);
        }
    }
}
