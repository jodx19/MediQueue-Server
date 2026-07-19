// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.API\Hubs\ClinicHub.cs
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace MediQueue.API.Hubs;

[Authorize]
public class ClinicHub : Hub
{
    private readonly ILogger<ClinicHub> _logger;

    public ClinicHub(ILogger<ClinicHub> logger)
    {
        _logger = logger;
    }

    // ─────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Extracts the TenantId claim emitted by TokenService.GenerateJwtToken.
    /// Returns null when the claim is missing (unauthenticated / SuperAdmin
    /// connections that are not scoped to a single tenant).
    /// </summary>
    private string? GetTenantId() =>
        Context.User?.FindFirst("TenantId")?.Value;

    /// <summary>
    /// The SignalR group that scopes all broadcasts to the current tenant.
    /// Every authenticated connection is added to this group in
    /// <see cref="OnConnectedAsync"/> so that no cross-tenant message ever
    /// reaches a different tenant's connections.
    /// </summary>
    private string TenantGroupName(string tenantId) => $"tenant:{tenantId}";

    // ─────────────────────────────────────────────────────────────
    // Connection lifecycle
    // ─────────────────────────────────────────────────────────────

    public override async Task OnConnectedAsync()
    {
        var connectionId = Context.ConnectionId;
        var userId       = Context.UserIdentifier ?? "anonymous";
        var tenantId     = GetTenantId();

        if (!string.IsNullOrEmpty(tenantId))
        {
            // Place connection inside the tenant-scoped group. All broadcasts
            // (BroadcastAsync, NotifyAppointmentConfirmed, …) target this group,
            // preventing cross-tenant data leakage.
            await Groups.AddToGroupAsync(connectionId, TenantGroupName(tenantId));
        }

        _logger.LogInformation(
            "SignalR client connected. ConnectionId={ConnectionId}, UserId={UserId}, TenantId={TenantId}",
            connectionId, userId, tenantId ?? "(none)");

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var connectionId = Context.ConnectionId;
        var userId       = Context.UserIdentifier ?? "anonymous";
        var tenantId     = GetTenantId();

        if (!string.IsNullOrEmpty(tenantId))
        {
            await Groups.RemoveFromGroupAsync(connectionId, TenantGroupName(tenantId));
        }

        if (exception is not null)
        {
            _logger.LogWarning(exception,
                "SignalR client disconnected with error. ConnectionId={ConnectionId}, UserId={UserId}, TenantId={TenantId}",
                connectionId, userId, tenantId ?? "(none)");
        }
        else
        {
            _logger.LogInformation(
                "SignalR client disconnected gracefully. ConnectionId={ConnectionId}, UserId={UserId}, TenantId={TenantId}",
                connectionId, userId, tenantId ?? "(none)");
        }

        await base.OnDisconnectedAsync(exception);
    }

    // ─────────────────────────────────────────────────────────────
    // Client-callable group management
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Subscribes the caller to their doctor-scoped group so they receive
    /// slot-update and appointment-status events for that doctor.
    /// The doctor sub-group is still inside the caller's tenant group; no
    /// cross-tenant isolation breach is possible because group membership
    /// requires an authenticated JWT.
    /// </summary>
    public async Task JoinDoctorGroup(string doctorId)
    {
        var tenantId = GetTenantId();
        if (string.IsNullOrEmpty(tenantId))
        {
            _logger.LogWarning(
                "JoinDoctorGroup rejected — no TenantId claim. ConnectionId={ConnectionId}",
                Context.ConnectionId);
            return;
        }

        // Use a tenant-scoped doctor group name so doctor IDs cannot collide
        // across tenants (even though UUIDs make this very unlikely).
        var groupName = $"doctor:{tenantId}:{doctorId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        _logger.LogDebug(
            "ConnectionId={ConnectionId} joined group {Group}", Context.ConnectionId, groupName);
    }

    /// <summary>
    /// Subscribes the caller to their patient-scoped group for prescription
    /// and appointment notifications. Only the patient themselves or staff
    /// should call this method; the server validates that a Patient role caller
    /// can only join their own PatientId group.
    /// </summary>
    public async Task JoinPatientGroup(string patientId)
    {
        var tenantId = GetTenantId();
        if (string.IsNullOrEmpty(tenantId))
        {
            _logger.LogWarning(
                "JoinPatientGroup rejected — no TenantId claim. ConnectionId={ConnectionId}",
                Context.ConnectionId);
            return;
        }

        var role         = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
        var callerPatientId = Context.User?.FindFirst("PatientId")?.Value;

        // Patients may only join their own group; Staff/Admin may join any.
        var isPatient = string.Equals(role, "Patient", StringComparison.OrdinalIgnoreCase);
        if (isPatient && !string.Equals(callerPatientId, patientId, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning(
                "JoinPatientGroup rejected — Patient {CallerId} attempted to join group for {TargetId}.",
                callerPatientId, patientId);
            return;
        }

        // Use a tenant-scoped patient group name.
        var groupName = $"patient:{tenantId}:{patientId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        _logger.LogDebug(
            "ConnectionId={ConnectionId} joined group {Group}", Context.ConnectionId, groupName);
    }

    public async Task JoinAdminGroup()
    {
        var role     = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
        var tenantId = GetTenantId();

        if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrEmpty(tenantId))
        {
            // Tenant-scoped admin group.
            await Groups.AddToGroupAsync(Context.ConnectionId, $"admin:{tenantId}");
        }
    }

    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }

    // ─────────────────────────────────────────────────────────────
    // Server-side broadcast helpers (called by server code only)
    // All use Clients.Group(tenantGroup) — NEVER Clients.All.
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Notifies all connected clients in the same tenant that an appointment
    /// was confirmed. Tenant isolation is enforced via the tenant SignalR group.
    /// </summary>
    public async Task NotifyAppointmentConfirmed(Guid appointmentId, string patientName)
    {
        var tenantId = GetTenantId();
        if (string.IsNullOrEmpty(tenantId)) return;

        await Clients
            .Group(TenantGroupName(tenantId))
            .SendAsync("NotifyAppointmentConfirmed", appointmentId, patientName);
    }

    /// <summary>
    /// Notifies all connected clients in the same tenant that an appointment
    /// was cancelled.
    /// </summary>
    public async Task NotifyAppointmentCancelled(Guid appointmentId, string reason)
    {
        var tenantId = GetTenantId();
        if (string.IsNullOrEmpty(tenantId)) return;

        await Clients
            .Group(TenantGroupName(tenantId))
            .SendAsync("NotifyAppointmentCancelled", appointmentId, reason);
    }

    /// <summary>
    /// Notifies all connected clients in the same tenant that an appointment
    /// was rescheduled.
    /// </summary>
    public async Task NotifyAppointmentRescheduled(Guid appointmentId, DateTime newDateTime)
    {
        var tenantId = GetTenantId();
        if (string.IsNullOrEmpty(tenantId)) return;

        await Clients
            .Group(TenantGroupName(tenantId))
            .SendAsync("NotifyAppointmentRescheduled", appointmentId, newDateTime);
    }

    /// <summary>
    /// Notifies all connected clients in the same tenant that a doctor's slot
    /// availability has changed.
    /// </summary>
    public async Task NotifySlotUpdated(Guid doctorId, DateOnly date)
    {
        var tenantId = GetTenantId();
        if (string.IsNullOrEmpty(tenantId)) return;

        var dateStr = date.ToString("yyyy-MM-dd");

        // Notify the tenant-scoped doctor group.
        await Clients
            .Group($"doctor:{tenantId}:{doctorId}")
            .SendAsync("NotifySlotUpdated", doctorId, dateStr);

        // Also notify all staff in the tenant (e.g., receptionists).
        await Clients
            .Group(TenantGroupName(tenantId))
            .SendAsync("NotifySlotUpdated", doctorId, dateStr);
    }

    /// <summary>
    /// Notifies a specific patient that their prescription is ready.
    /// Uses the tenant-scoped patient group to prevent cross-tenant leakage.
    /// </summary>
    public async Task NotifyPrescriptionReady(Guid patientId, Guid visitId)
    {
        var tenantId = GetTenantId();
        if (string.IsNullOrEmpty(tenantId)) return;

        await Clients
            .Group($"patient:{tenantId}:{patientId}")
            .SendAsync("NotifyPrescriptionReady", patientId, visitId);
    }
}
