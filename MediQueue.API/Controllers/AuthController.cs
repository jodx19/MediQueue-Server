// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.API\Controllers\AuthController.cs
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediQueue.Application.Auth.Commands;
using MediQueue.Application.Auth.DTOs;
using MediQueue.Application.Common;

using Microsoft.AspNetCore.RateLimiting;

namespace MediQueue.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("AuthPolicy")]
public class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginCommand command)
    {
        var result = await _sender.Send(command);
        if (!result.IsSuccess)
        {
            return Unauthorized(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpPost("patient-login")]
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    [EnableRateLimiting("PatientLoginPolicy")]
    public async Task<ActionResult<AuthResponseDto>> PatientLogin([FromBody] PatientLoginCommand command)
    {
        var result = await _sender.Send(command);
        if (!result.IsSuccess)
        {
            return Unauthorized(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult> Register([FromBody] RegisterCommand command)
    {
        var result = await _sender.Send(command);
        if (!result.IsSuccess)
        {
            return BadRequest(result.Error);
        }

        return Ok();
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] RefreshTokenCommand command)
    {
        var result = await _sender.Send(command);
        if (!result.IsSuccess)
        {
            return Unauthorized(result.Error);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Revokes the caller's refresh token. Requires a valid access token —
    /// logout is only meaningful for the currently-authenticated principal.
    /// Returns 204 unconditionally: caller MUST NOT be able to distinguish
    /// "user existed and was revoked" from "user did not exist" (silent success).
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        await _sender.Send(new LogoutCommand(), ct);
        return NoContent();
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [EnableRateLimiting("AuthPolicy")]
    public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command)
    {
        var result = await _sender.Send(command);
        if (!result.IsSuccess)
        {
            return BadRequest(result.Error);
        }

        return Ok();
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    [EnableRateLimiting("AuthPolicy")]
    public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordCommand command)
    {
        var result = await _sender.Send(command);
        if (!result.IsSuccess)
        {
            return BadRequest(result.Error);
        }

        return Ok();
    }
}
