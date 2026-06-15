using System;
using MediQueue.Application.Interfaces;

namespace MediQueue.API.Services;

public class TenantContext : ITenantContext
{
    public Guid TenantId { get; set; } = Guid.Empty;
    public string Subdomain { get; set; } = string.Empty;
}
