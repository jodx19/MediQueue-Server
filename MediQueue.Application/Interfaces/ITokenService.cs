using System;
using System.Collections.Generic;
using System.Security.Claims;
using MediQueue.Domain.Entities;

namespace MediQueue.Application.Interfaces;

public interface ITokenService
{
    string GenerateJwtToken(AppUser user);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
