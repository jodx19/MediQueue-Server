// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.API\Controllers\AuthController.cs
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MediQueue.Application.Auth.DTOs;
using MediQueue.Application.Interfaces;
using MediQueue.Application.Common;

namespace MediQueue.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiVersion("1.0")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        var result = await _authService.LoginAsync(request);
        if (!result.IsSuccess)
        {
            return Unauthorized(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register([FromBody] RegisterRequestDto request)
    {
        var result = await _authService.RegisterAsync(request);
        if (!result.IsSuccess)
        {
            return BadRequest(result.Error);
        }

        return Ok();
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        var result = await _authService.RefreshTokenAsync(request);
        if (!result.IsSuccess)
        {
            return Unauthorized(result.Error);
        }

        return Ok(result.Value);
    }
}
