using MediatR;
using Microsoft.AspNetCore.Mvc;
using MediQueue.Application.Common;
using Microsoft.Extensions.DependencyInjection;

namespace MediQueue.API.Controllers;

/// <summary>
/// Base class for all API controllers providing common functionality and result handling.
/// Controllers return DTOs directly; the global <c>ApiResponseFilter</c>
/// wraps every <c>ObjectResult</c> into the standard <c>ApiResponse&lt;T&gt;</c> envelope.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    private ISender? _sender;

    /// <summary>
    /// MediatR sender for dispatching commands and queries.
    /// Uses property injection via RequestServices to keep constructors clean in derived classes.
    /// </summary>
    protected ISender Sender => _sender ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    /// <summary>
    /// Maps a successful <see cref="Result{T}"/> to a 200 OK with the value,
    /// or a failed result to the appropriate HTTP error status.
    /// The global <c>ApiResponseFilter</c> wraps the body automatically.
    /// </summary>
    protected ActionResult HandleResult<T>(Result<T> result)
    {
        if (result is null || (!result.IsSuccess))
            return MapFailure(result?.Error ?? "An unexpected error occurred.");

        if (result.Value is null)
            return NotFound();

        return Ok(result.Value);
    }

    /// <summary>
    /// Processes a non-generic <see cref="Result"/> (void commands).
    /// </summary>
    protected ActionResult HandleResult(Result result)
    {
        if (result is null || (!result.IsSuccess))
            return MapFailure(result?.Error ?? "An unexpected error occurred.");

        return Ok(new { });
    }

    private ActionResult MapFailure(string error)
    {
        if (error.Contains("not found", StringComparison.OrdinalIgnoreCase))
            return NotFound(error);

        if (error.Contains("unauthorized", StringComparison.OrdinalIgnoreCase))
            return Unauthorized(error);

        if (error.Contains("conflict", StringComparison.OrdinalIgnoreCase))
            return Conflict(error);

        if (error.Contains("forbidden", StringComparison.OrdinalIgnoreCase))
            return Forbid();

        return BadRequest(error);
    }

    protected ActionResult Success<T>(T data)
    {
        return Ok(data);
    }

    protected ActionResult Failure(string error)
    {
        return BadRequest(error);
    }
}
