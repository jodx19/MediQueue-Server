using System.Net;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using MediQueue.Application.Common.Behaviors;
using MediQueue.Domain.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;

namespace MediQueue.API.Middleware;

/// <summary>
/// Global exception handling middleware that provides RFC 7807 compliant error responses
/// Handles domain exceptions, validation exceptions, and unexpected errors with enterprise-grade logging
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.Clear();
        
        var problemDetails = CreateProblemDetails(exception);
        
        context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = _environment.IsDevelopment(),
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        var json = JsonSerializer.Serialize(problemDetails, jsonOptions);
        
        await context.Response.WriteAsync(json);
        
        // Log the exception with structured logging
        LogException(exception, problemDetails, context);
    }

    private ProblemDetails CreateProblemDetails(Exception exception)
    {
        return exception switch
        {
            ValidationException validationEx => CreateValidationProblemDetails(validationEx),
            DomainException domainEx => CreateDomainProblemDetails(domainEx),
            ArgumentException argEx => CreateArgumentProblemDetails(argEx),
            UnauthorizedAccessException => CreateUnauthorizedProblemDetails(),
            NotImplementedException => CreateNotImplementedProblemDetails(),
            OperationCanceledException => CreateOperationCancelledProblemDetails(),
            TimeoutException timeoutEx => CreateTimeoutProblemDetails(timeoutEx),
            HttpRequestException httpEx => CreateHttpRequestProblemDetails(httpEx),
            _ => CreateInternalServerErrorProblemDetails(exception)
        };
    }

    private static ProblemDetails CreateValidationProblemDetails(ValidationException exception)
    {
        var problemDetails = new ValidationProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Type = "https://httpstatuses.com/400",
            Title = "Validation Failed",
            Detail = "One or more validation errors occurred.",
            Instance = Guid.NewGuid().ToString()
        };

        foreach (var group in exception.Errors.GroupBy(e => e.PropertyName))
        {
            var errors = new List<string>();
            foreach (var error in group)
            {
                errors.Add(error.ErrorMessage);
            }
            problemDetails.Errors[group.Key] = errors.ToArray();
        }

        var errorList = new List<string>();
        foreach (var error in exception.Errors)
        {
            errorList.Add($"{error.PropertyName}: {error.ErrorMessage}");
        }
        problemDetails.Extensions["validationErrors"] = errorList;

        return problemDetails;
    }

    private static ProblemDetails CreateDomainProblemDetails(DomainException exception)
    {
        var statusCode = exception switch
        {
            NotFoundException => StatusCodes.Status404NotFound,
            ForbiddenException => StatusCodes.Status403Forbidden,
            ConflictException => StatusCodes.Status409Conflict,
            InvalidStateException => StatusCodes.Status400BadRequest,
            BusinessRuleViolationException => StatusCodes.Status422UnprocessableEntity,
            _ => StatusCodes.Status400BadRequest
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Type = $"https://httpstatuses.com/{statusCode}",
            Title = GetTitleForStatusCode(statusCode),
            Detail = exception.Message,
            Instance = Guid.NewGuid().ToString(),
            Extensions =
            {
                ["errorCode"] = exception.ErrorCode,
                ["details"] = exception.Details,
                ["exceptionType"] = exception.GetType().Name
            }
        };

        // Add domain-specific context
        if (exception is NotFoundException notFoundEx)
        {
            problemDetails.Extensions["entityType"] = notFoundEx.EntityType;
            problemDetails.Extensions["entityId"] = notFoundEx.EntityId;
        }
        else if (exception is ConflictException conflictEx)
        {
            problemDetails.Extensions["resourceType"] = conflictEx.ResourceType;
            problemDetails.Extensions["resourceId"] = conflictEx.ResourceId;
        }

        return problemDetails;
    }

    private static ProblemDetails CreateArgumentProblemDetails(ArgumentException exception)
    {
        return new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Type = "https://httpstatuses.com/400",
            Title = "Invalid Argument",
            Detail = exception.Message,
            Instance = Guid.NewGuid().ToString(),
            Extensions =
            {
                ["parameterName"] = exception.ParamName,
                ["exceptionType"] = exception.GetType().Name
            }
        };
    }

    private static ProblemDetails CreateUnauthorizedProblemDetails()
    {
        return new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Type = "https://httpstatuses.com/401",
            Title = "Unauthorized",
            Detail = "Authentication is required to access this resource.",
            Instance = Guid.NewGuid().ToString(),
            Extensions =
            {
                ["exceptionType"] = "UnauthorizedAccessException"
            }
        };
    }

    private static ProblemDetails CreateNotImplementedProblemDetails()
    {
        return new ProblemDetails
        {
            Status = StatusCodes.Status501NotImplemented,
            Type = "https://httpstatuses.com/501",
            Title = "Not Implemented",
            Detail = "This feature is not yet implemented.",
            Instance = Guid.NewGuid().ToString(),
            Extensions =
            {
                ["exceptionType"] = "NotImplementedException"
            }
        };
    }

    private static ProblemDetails CreateOperationCancelledProblemDetails()
    {
        return new ProblemDetails
        {
            Status = StatusCodes.Status408RequestTimeout,
            Type = "https://httpstatuses.com/408",
            Title = "Request Timeout",
            Detail = "The operation was cancelled due to timeout.",
            Instance = Guid.NewGuid().ToString(),
            Extensions =
            {
                ["exceptionType"] = "OperationCanceledException"
            }
        };
    }

    private static ProblemDetails CreateTimeoutProblemDetails(TimeoutException exception)
    {
        return new ProblemDetails
        {
            Status = StatusCodes.Status408RequestTimeout,
            Type = "https://httpstatuses.com/408",
            Title = "Request Timeout",
            Detail = $"The operation timed out: {exception.Message}",
            Instance = Guid.NewGuid().ToString(),
            Extensions =
            {
                ["exceptionType"] = "TimeoutException",
                ["timeoutDuration"] = exception.Message
            }
        };
    }

    private static ProblemDetails CreateHttpRequestProblemDetails(HttpRequestException exception)
    {
        var statusCode = (int)(exception.StatusCode ?? HttpStatusCode.InternalServerError);
        
        return new ProblemDetails
        {
            Status = statusCode,
            Type = $"https://httpstatuses.com/{statusCode}",
            Title = GetTitleForStatusCode(statusCode),
            Detail = $"HTTP request failed: {exception.Message}",
            Instance = Guid.NewGuid().ToString(),
            Extensions =
            {
                ["exceptionType"] = "HttpRequestException",
                ["httpStatusCode"] = statusCode
            }
        };
    }

    private ProblemDetails CreateInternalServerErrorProblemDetails(Exception exception)
    {
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Type = "https://httpstatuses.com/500",
            Title = "Internal Server Error",
            Detail = _environment.IsDevelopment() 
                ? exception.Message 
                : "An unexpected error occurred. Please try again later.",
            Instance = Guid.NewGuid().ToString(),
            Extensions =
            {
                ["exceptionType"] = exception.GetType().Name,
                ["requestId"] = Activity.Current?.Id ?? Guid.NewGuid().ToString()
            }
        };

        if (_environment.IsDevelopment())
        {
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
            problemDetails.Extensions["source"] = exception.Source;
            problemDetails.Extensions["innerException"] = exception.InnerException?.Message;
        }

        return problemDetails;
    }

    private void LogException(Exception exception, ProblemDetails problemDetails, HttpContext context)
    {
        var logLevel = exception switch
        {
            ValidationException => LogLevel.Warning,
            DomainException => LogLevel.Warning,
            ArgumentException => LogLevel.Warning,
            UnauthorizedAccessException => LogLevel.Warning,
            OperationCanceledException => LogLevel.Information,
            TimeoutException => LogLevel.Warning,
            HttpRequestException => LogLevel.Warning,
            _ => LogLevel.Error
        };

        var logContext = new Dictionary<string, object>
        {
            ["ProblemType"] = problemDetails.Type,
            ["StatusCode"] = problemDetails.Status,
            ["Message"] = problemDetails.Detail,
            ["InstanceId"] = problemDetails.Instance,
            ["RequestId"] = context.TraceIdentifier,
            ["Path"] = context.Request.Path,
            ["Method"] = context.Request.Method,
            ["UserAgent"] = context.Request.Headers["User-Agent"].ToString(),
            ["RemoteIpAddress"] = context.Connection.RemoteIpAddress?.ToString()
        };

        // Add exception-specific context
        if (exception is ValidationException validationEx)
        {
            logContext["ValidationErrors"] = validationEx.Errors.Select(e => new
            {
                Property = e.PropertyName,
                Message = e.ErrorMessage
            }).ToArray();
        }
        else if (exception is DomainException domainEx)
        {
            logContext["ErrorCode"] = domainEx.ErrorCode;
            logContext["ErrorDetails"] = domainEx.Details;
        }

        // Add request context
        if (context.User.Identity?.IsAuthenticated == true)
        {
            logContext["UserId"] = context.User.FindFirst("sub")?.Value;
            logContext["UserName"] = context.User.FindFirst("name")?.Value;
        }

        _logger.Log(
            logLevel,
            exception,
            "Exception handled: {ProblemType} ({StatusCode}) - {Message}",
            problemDetails.Type,
            problemDetails.Status,
            problemDetails.Detail);

        // Log structured context
        foreach (var kvp in logContext)
        {
            _logger.LogDebug("Context: {Key} = {Value}", kvp.Key, kvp.Value);
        }
    }

    private static string GetTitleForStatusCode(int statusCode)
    {
        return statusCode switch
        {
            400 => "Bad Request",
            401 => "Unauthorized",
            403 => "Forbidden",
            404 => "Not Found",
            408 => "Request Timeout",
            409 => "Conflict",
            422 => "Unprocessable Entity",
            500 => "Internal Server Error",
            501 => "Not Implemented",
            _ => "Error"
        };
    }
}

/// <summary>
/// Extension methods for registering the custom exception handling middleware
/// </summary>
public static class CustomExceptionMiddlewareExtensions
{
    /// <summary>
    /// Adds the global custom exception handling middleware to the request pipeline
    /// </summary>
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionMiddleware>();
    }
}
