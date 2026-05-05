// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Common\Behaviors\LoggingBehavior.cs
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using MediQueue.Application.Common;

namespace MediQueue.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that logs request execution and results.
/// </summary>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        _logger.LogInformation("Starting request {RequestName} at {StartTime}", requestName, DateTime.UtcNow);

        var stopwatch = Stopwatch.StartNew();
        var response = await next();
        stopwatch.Stop();

        _logger.LogInformation("Completed request {RequestName} in {ElapsedMilliseconds}ms", requestName, stopwatch.ElapsedMilliseconds);

        if (stopwatch.ElapsedMilliseconds > 500)
        {
            _logger.LogWarning("Long running request {RequestName} took {ElapsedMilliseconds}ms", requestName, stopwatch.ElapsedMilliseconds);
        }

        if (response is Result result && result.IsFailure)
        {
            _logger.LogError("Request {RequestName} failed with error: {Error}", requestName, result.Error);
        }

        return response;
    }
}
