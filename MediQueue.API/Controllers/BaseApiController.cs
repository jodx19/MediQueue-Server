using MediatR;
using Microsoft.AspNetCore.Mvc;
using MediQueue.Application.Common;
using Microsoft.Extensions.DependencyInjection;
using MediQueue.API.Models;

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
    /// Maps success/failure to HTTP status codes and wraps them in <see cref="ApiResponse{T}"/>.
    /// </summary>
    protected ActionResult HandleResult<T>(Result<T> result)
    {
        if (result == null) return NotFound(ApiResponse<object>.Failure("Resource not found."));
        
        if (result.IsSuccess)
        {
            if (result.Value == null) return NotFound(ApiResponse<object>.Failure("Resource not found."));
            return Ok(ApiResponse<T>.Success(result.Value));
        }

        return MapFailure(result.Error!);
    }

    /// <summary>
    /// Processes a non-generic <see cref="Result"/> and returns the appropriate <see cref="ActionResult"/>.
    /// </summary>
    protected ActionResult HandleResult(Result result)
    {
        if (result == null) return NotFound(ApiResponse<object>.Failure("Resource not found."));
        
        if (result.IsSuccess) 
            return Ok(ApiResponse<object>.Success(new { }, "Operation completed successfully."));

        return MapFailure(result.Error!);
    }

    private ActionResult MapFailure(string error)
    {
        if (error.Contains("not found", StringComparison.OrdinalIgnoreCase))
            return NotFound(ApiResponse<object>.Failure(error));

        if (error.Contains("unauthorized", StringComparison.OrdinalIgnoreCase))
            return Unauthorized(ApiResponse<object>.Failure(error));

        if (error.Contains("conflict", StringComparison.OrdinalIgnoreCase))
            return Conflict(ApiResponse<object>.Failure(error));

        if (error.Contains("forbidden", StringComparison.OrdinalIgnoreCase))
            return Forbid(); // Or custom response

        return BadRequest(ApiResponse<object>.Failure(error));
    }

    protected ActionResult Success<T>(T data, string? message = null)
    {
        return Ok(ApiResponse<T>.Success(data, message));
    }

    protected ActionResult Failure(string error, string? message = null)
    {
        return BadRequest(ApiResponse<object>.Failure(error, message));
    }
}
