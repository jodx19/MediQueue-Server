using MediatR;
using Microsoft.AspNetCore.Mvc;
using MediQueue.Application.Common;
using Microsoft.Extensions.DependencyInjection;

namespace MediQueue.API.Controllers;

/// <summary>
/// Base class for all API controllers providing common functionality and result handling.
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
    /// Processes a <see cref="Result{T}"/> and returns the appropriate <see cref="ActionResult"/>.
    /// Maps success/failure to HTTP status codes following REST best practices.
    /// </summary>
    protected ActionResult HandleResult<T>(Result<T> result)
    {
        if (result == null) return NotFound();
        
        if (result.IsSuccess)
        {
            if (result.Value == null) return NotFound();
            return Ok(result.Value);
        }

        // Logic for mapping failure messages to status codes
        if (result.Error.Contains("not found", StringComparison.OrdinalIgnoreCase))
            return NotFound(new { error = result.Error });

        if (result.Error.Contains("unauthorized", StringComparison.OrdinalIgnoreCase))
            return Unauthorized(new { error = result.Error });

        if (result.Error.Contains("conflict", StringComparison.OrdinalIgnoreCase))
            return Conflict(new { error = result.Error });

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Processes a non-generic <see cref="Result"/> and returns the appropriate <see cref="ActionResult"/>.
    /// </summary>
    protected ActionResult HandleResult(Result result)
    {
        if (result == null) return NotFound();
        
        if (result.IsSuccess) return NoContent();

        if (result.Error.Contains("not found", StringComparison.OrdinalIgnoreCase))
            return NotFound(new { error = result.Error });

        return BadRequest(new { error = result.Error });
    }
}
