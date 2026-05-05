using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using MediQueue.Application.Interfaces;
using MediQueue.Infrastructure.Hubs;

namespace MediQueue.Infrastructure.ExternalServices;

public class SignalRRealtimeService : IRealtimeService
{
    private readonly IHubContext<ClinicHub> _hubContext;

    public SignalRRealtimeService(IHubContext<ClinicHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendToPatientAsync(Guid patientId, string eventName, object data)
    {
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
        await _hubContext.Clients.All.SendAsync(eventName, data);
    }
}
