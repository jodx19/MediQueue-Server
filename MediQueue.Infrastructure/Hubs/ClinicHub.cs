// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Infrastructure\Hubs\ClinicHub.cs
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace MediQueue.Infrastructure.Hubs;

/// <summary>
/// Real-time SignalR hub for the MediQueue clinic system.
/// Groups:
///   doctor:{id}   → messages for a specific doctor
///   patient:{id}  → messages for a specific patient
///   admin         → broadcast to all admin clients
/// </summary>
[Authorize]
public class ClinicHub : Hub
{
    private readonly ILogger<ClinicHub> _logger;

    public ClinicHub(ILogger<ClinicHub> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public override async Task OnConnectedAsync()
    {
        var connectionId = Context.ConnectionId;
        var userId = Context.UserIdentifier ?? "anonymous";

        _logger.LogInformation(
            "SignalR client connected. ConnectionId={ConnectionId}, UserId={UserId}",
            connectionId, userId);

        await base.OnConnectedAsync();
    }

    /// <inheritdoc/>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var connectionId = Context.ConnectionId;
        var userId = Context.UserIdentifier ?? "anonymous";

        if (exception is not null)
        {
            _logger.LogWarning(exception,
                "SignalR client disconnected with error. ConnectionId={ConnectionId}, UserId={UserId}",
                connectionId, userId);
        }
        else
        {
            _logger.LogInformation(
                "SignalR client disconnected gracefully. ConnectionId={ConnectionId}, UserId={UserId}",
                connectionId, userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Client calls this to subscribe to a doctor's real-time updates.
    /// </summary>
    public async Task JoinDoctorGroup(string doctorId)
    {
        var groupName = $"doctor:{doctorId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _logger.LogDebug("Connection {ConnectionId} joined group {Group}", Context.ConnectionId, groupName);
    }

    /// <summary>
    /// Client calls this to subscribe to a patient's real-time updates.
    /// </summary>
    public async Task JoinPatientGroup(string patientId)
    {
        var groupName = $"patient:{patientId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _logger.LogDebug("Connection {ConnectionId} joined group {Group}", Context.ConnectionId, groupName);
    }

    /// <summary>
    /// Client calls this to subscribe to the admin broadcast group.
    /// </summary>
    public async Task JoinAdminGroup()
    {
        var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
        if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            const string groupName = "admin";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            _logger.LogDebug("Connection {ConnectionId} joined admin group", Context.ConnectionId);
        }
    }

    /// <summary>
    /// Removes the connection from a named group.
    /// </summary>
    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        _logger.LogDebug("Connection {ConnectionId} left group {Group}", Context.ConnectionId, groupName);
    }

    /// <summary>
    /// Sends a message to all members of a group.
    /// Exposed so server-side code can call it directly on hub context if needed.
    /// </summary>
    public async Task SendToGroup(string groupName, string eventName, object data)
    {
        await Clients.Group(groupName).SendAsync(eventName, data);
    }

    public async Task NotifyAppointmentConfirmed(Guid appointmentId, string patientName)
        => await Clients.All.SendAsync("NotifyAppointmentConfirmed", appointmentId, patientName);

    public async Task NotifyAppointmentCancelled(Guid appointmentId, string reason)
        => await Clients.All.SendAsync("NotifyAppointmentCancelled", appointmentId, reason);

    public async Task NotifyAppointmentRescheduled(Guid appointmentId, DateTime newDateTime)
        => await Clients.All.SendAsync("NotifyAppointmentRescheduled", appointmentId, newDateTime);

    public async Task NotifySlotUpdated(Guid doctorId, DateOnly date)
        => await Clients.All.SendAsync("NotifySlotUpdated", doctorId, date.ToString("yyyy-MM-dd"));

    public async Task NotifyPrescriptionReady(Guid patientId, Guid visitId)
        => await Clients.Group($"patient:{patientId}").SendAsync("NotifyPrescriptionReady", patientId, visitId);
}
