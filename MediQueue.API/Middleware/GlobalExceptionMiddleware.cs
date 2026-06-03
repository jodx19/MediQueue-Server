using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MediQueue.API.Models;
using FluentValidation;
using NotFoundException = MediQueue.Domain.Common.Exceptions.NotFoundException;
using BusinessRuleViolationException = MediQueue.Domain.Common.Exceptions.BusinessRuleViolationException;
using CommonDomainException = MediQueue.Domain.Common.Exceptions.DomainException;
using MediQueue.Domain.Exceptions;

namespace MediQueue.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
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
        context.Response.ContentType = "application/json";

        var (statusCode, message, errors) = exception switch
        {
            ValidationException ve => (
                StatusCodes.Status422UnprocessableEntity,
                "Validation failed",
                ve.Errors.Select(e => e.ErrorMessage).ToList()
            ),

            NotFoundException nfe => (
                StatusCodes.Status404NotFound,
                nfe.Message,
                (List<string>?)null
            ),

            BusinessRuleViolationException bre => (
                StatusCodes.Status409Conflict,
                bre.Message,
                (List<string>?)null
            ),

            AppointmentConflictException ace => (
                StatusCodes.Status409Conflict,
                ace.Message,
                (List<string>?)null
            ),

            InvalidAppointmentStatusException iase => (
                StatusCodes.Status400BadRequest,
                iase.Message,
                (List<string>?)null
            ),

            UnauthorizedAccessException => (
                StatusCodes.Status401Unauthorized,
                "Unauthorized access",
                (List<string>?)null
            ),

            CommonDomainException de => (
                StatusCodes.Status400BadRequest,
                de.Message,
                (List<string>?)null
            ),

            _ => (
                StatusCodes.Status500InternalServerError,
                _env.IsDevelopment() ? exception.Message : "An unexpected error occurred",
                new List<string> { exception.Message }
            )
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
        }

        context.Response.StatusCode = statusCode;

        var response = ApiResponse<object>.Failure(errors ?? new List<string>(), message);
        await context.Response.WriteAsJsonAsync(response);
    }
}
