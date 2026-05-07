using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using MediQueue.Application.Interfaces;

namespace MediQueue.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    
    public string? Username => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name);
    
    public string? Role => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Role);
    
    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public Guid? DoctorId 
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User?.FindFirstValue("DoctorId");
            return Guid.TryParse(claim, out var guid) ? guid : null;
        }
    }

    public Guid? PatientId 
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User?.FindFirstValue("PatientId");
            return Guid.TryParse(claim, out var guid) ? guid : null;
        }
    }
}
