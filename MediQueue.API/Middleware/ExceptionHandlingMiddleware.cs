using System.Net;
using System.Text.Json;
using MediQueue.Application.Common.Behaviors;
using MediQueue.Domain.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;

namespace MediQueue.API.Middleware;

/// <summary>
/// Global exception handling middleware that provides RFC 7807 compliant error responses
/// Handles domain exceptions, validation exceptions, and unexpected errors
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
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
            WriteIndented = _environment.IsDevelopment()
        };

        var json = JsonSerializer.Serialize(problemDetails, jsonOptions);
        
        await context.Response.WriteAsync(json);
        
        // Log the exception
        LogException(exception, problemDetails);
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

        foreach (var error in exception.Errors)
        {
            if (!problemDetails.Errors.ContainsKey(error.PropertyName))
            {
                problemDetails.Errors[error.PropertyName] = new[] { error.ErrorMessage };
            }
            else
            {
                var errors = problemDetails.Errors[error.PropertyName].ToList();
                errors.Add(error.ErrorMessage);
                problemDetails.Errors[error.PropertyName] = errors.ToArray();
            }
        }

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

        return new ProblemDetails
        {
            Status = statusCode,
            Type = $"https://httpstatuses.com/{statusCode}",
            Title = GetTitleForStatusCode(statusCode),
            Detail = exception.Message,
            Instance = Guid.NewGuid().ToString(),
            Extensions =
            {
                ["errorCode"] = exception.ErrorCode,
                ["details"] = exception.Details
            }
        };
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
                ["parameterName"] = exception.ParamName
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
            Instance = Guid.NewGuid().ToString()
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
            Instance = Guid.NewGuid().ToString()
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
            Instance = Guid.NewGuid().ToString()
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
            Instance = Guid.NewGuid().ToString()
        };

        if (_environment.IsDevelopment())
        {
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
            problemDetails.Extensions["exceptionType"] = exception.GetType().Name;
        }

        return problemDetails;
    }

    private void LogException(Exception exception, ProblemDetails problemDetails)
    {
        var logLevel = exception switch
        {
            ValidationException => LogLevel.Warning,
            DomainException => LogLevel.Warning,
            ArgumentException => LogLevel.Warning,
            UnauthorizedAccessException => LogLevel.Warning,
            OperationCanceledException => LogLevel.Information,
            _ => LogLevel.Error
        };

        _logger.Log(
            logLevel,
            exception,
            "Exception handled: {ProblemType} ({StatusCode}) - {Message}",
            problemDetails.Type,
            problemDetails.Status,
            problemDetails.Detail);
    }

    private static string GetTitleForStatusCode(int statusCode)
    {
        return statusCode switch
        {
            400 => "Bad Request",
            401 => "Unauthorized",
            403 => "Forbidden",
            404 => "Not Found",
            409 => "Conflict",
            422 => "Unprocessable Entity",
            500 => "Internal Server Error",
            501 => "Not Implemented",
            508 => "Request Timeout",
            _ => "Error"
        };
    }
}

/// <summary>
/// Extension methods for registering the exception handling middleware
/// </summary>
public static class ExceptionHandlingMiddlewareExtensions
{
    /// <summary>
    /// Adds the global exception handling middleware to the request pipeline
    /// </summary>
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
