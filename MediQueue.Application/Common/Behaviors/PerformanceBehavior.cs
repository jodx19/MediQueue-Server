// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Common\Behaviors\PerformanceBehavior.cs
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MediQueue.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that tracks execution time and warns if it exceeds a threshold.
/// </summary>
public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;

    public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        
        var response = await next();
        
        stopwatch.Stop();
        
        if (stopwatch.ElapsedMilliseconds > 1000)
        {
            var requestName = typeof(TRequest).Name;
            _logger.LogWarning("MediQueue Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds) {@Request}",
                requestName, stopwatch.ElapsedMilliseconds, request);
        }

        return response;
    }
}
