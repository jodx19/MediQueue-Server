using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using MediQueue.Application.Interfaces;
using MediQueue.API.Hubs;

namespace MediQueue.API.Services;

/// <summary>
/// Infrastructure implementation of <see cref="IRealtimeService"/> using SignalR.
/// Every method targets a tenant-scoped group so that no event ever leaks
/// across tenant boundaries.  Group membership is established in
/// <see cref="ClinicHub.OnConnectedAsync"/> from the JWT "TenantId" claim.
/// </summary>
public class SignalRRealtimeService : IRealtimeService
{
    private readonly IHubContext<ClinicHub> _hubContext;
    private readonly ITenantContext         _tenantContext;

    public SignalRRealtimeService(
        IHubContext<ClinicHub> hubContext,
        ITenantContext         tenantContext)
    {
        _hubContext    = hubContext;
        _tenantContext = tenantContext;
    }

    // ─── Private helpers ──────────────────────────────────────────

    /// <summary>
    /// The SignalR group that contains every connection belonging to the current
    /// request's tenant.  Populated by ClinicHub.OnConnectedAsync.
    /// </summary>
    private string TenantGroup => $"tenant:{_tenantContext.TenantId}";

    /// <summary>
    /// Tenant-scoped doctor group.  ClinicHub.JoinDoctorGroup uses the same
    /// naming convention so subscriptions land in exactly this group.
    /// </summary>
    private string DoctorGroup(Guid doctorId) =>
        $"doctor:{_tenantContext.TenantId}:{doctorId}";

    /// <summary>
    /// Tenant-scoped patient group.  ClinicHub.JoinPatientGroup uses the same
    /// naming convention.
    /// </summary>
    private string PatientGroup(Guid patientId) =>
        $"patient:{_tenantContext.TenantId}:{patientId}";

    // ─── IRealtimeService implementation ─────────────────────────

    /// <summary>
    /// Sends an event to a specific patient's group.  The patient group is
    /// already tenant-scoped by the group-name convention.
    /// </summary>
    public async Task SendToPatientAsync(Guid patientId, string eventName, object data)
    {
        await _hubContext.Clients
            .Group(PatientGroup(patientId))
            .SendAsync(eventName, data);
    }

    /// <summary>
    /// Sends an event to a specific doctor's group.
    /// </summary>
    public async Task SendToDoctorAsync(Guid doctorId, string eventName, object data)
    {
        await _hubContext.Clients
            .Group(DoctorGroup(doctorId))
            .SendAsync(eventName, data);
    }

    /// <summary>
    /// Sends an event to an arbitrary group name.  Callers are responsible for
    /// using tenant-scoped group names; raw group names are supported for
    /// advanced use-cases (e.g. admin channels).
    /// </summary>
    public async Task SendToGroupAsync(string groupName, string eventName, object data)
    {
        await _hubContext.Clients
            .Group(groupName)
            .SendAsync(eventName, data);
    }

    /// <summary>
    /// Broadcasts an event to ALL connections of the current tenant only.
    /// Uses <see cref="TenantGroup"/> — never <c>Clients.All</c>.
    /// </summary>
    public async Task BroadcastAsync(string eventName, object data)
    {
        if (_tenantContext.TenantId == Guid.Empty)
            return; // No tenant context — safe no-op.

        await _hubContext.Clients
            .Group(TenantGroup)
            .SendAsync(eventName, data);
    }

    /// <summary>
    /// Notifies that a doctor's slot availability has changed.
    /// Sends to both the doctor's specific group and the tenant-wide group
    /// so that receptionists and admins are also informed.
    /// </summary>
    public async Task NotifySlotUpdatedAsync(Guid doctorId, DateOnly date)
    {
        if (_tenantContext.TenantId == Guid.Empty)
            return;

        var dateStr     = date.ToString("yyyy-MM-dd");
        var doctorGroup = DoctorGroup(doctorId);
        var tenantGroup = TenantGroup;

        // Doctor-specific subscribers.
        await _hubContext.Clients
            .Group(doctorGroup)
            .SendAsync("NotifySlotUpdated", doctorId, dateStr);

        // Tenant-wide (receptionist, admin) subscribers — avoids duplicates
        // for non-doctor connections; doctor connections receive it once via
        // their own group only if they did NOT also join the tenant group.
        await _hubContext.Clients
            .Group(tenantGroup)
            .SendAsync("NotifySlotUpdated", doctorId, dateStr);
    }

    /// <summary>
    /// Notifies a specific patient that their prescription is ready.
    /// </summary>
    public async Task NotifyPrescriptionReadyAsync(Guid patientId, Guid visitId)
    {
        await _hubContext.Clients
            .Group(PatientGroup(patientId))
            .SendAsync("NotifyPrescriptionReady", patientId, visitId);
    }

    /// <summary>
    /// Notifies the current tenant's connections that an appointment was cancelled.
    /// </summary>
    public async Task NotifyAppointmentCancelledAsync(Guid appointmentId, string reason)
    {
        if (_tenantContext.TenantId == Guid.Empty)
            return;

        await _hubContext.Clients
            .Group(TenantGroup)
            .SendAsync("NotifyAppointmentCancelled", appointmentId, reason);
    }
}
