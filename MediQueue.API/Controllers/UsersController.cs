using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediQueue.Application.Auth.Commands;
using MediQueue.Application.Auth.Queries;

namespace MediQueue.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminOnly")]
[Produces("application/json")]
public class UsersController : BaseApiController
{
    /// <summary>Get all system users (Admin only).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<UserListItemDto>>> GetAll()
    {
        var result = await Sender.Send(new GetAllUsersQuery());
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    /// <summary>Deactivate a system user (Admin only).</summary>
    [HttpPut("{email}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Deactivate(string email, CancellationToken ct)
    {
        var result = await Sender.Send(new DeactivateUserCommand(email), ct);
        return result.IsSuccess ? Ok() : BadRequest(result.Error);
    }
}
