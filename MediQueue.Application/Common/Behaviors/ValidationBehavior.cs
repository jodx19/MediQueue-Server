// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Common\Behaviors\ValidationBehavior.cs
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using MediQueue.Application.Common;

namespace MediQueue.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that runs all validators for a request.
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            var failures = validationResults
                .Where(r => r.Errors.Any())
                .SelectMany(r => r.Errors)
                .ToList();

            if (failures.Any())
            {
                var errors = string.Join("; ", failures.Select(f => f.ErrorMessage));
                
                // We need to return a Result.Failure. We must figure out if TResponse is Result or Result<T>.
                // Since this is a pipeline behavior, we can use reflection or type constraints if we had restricted TResponse.
                // Assuming all commands/queries return Result or Result<T>.
                if (typeof(TResponse) == typeof(Result))
                {
                    return (TResponse)(object)Result.Failure(errors);
                }
                
                if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
                {
                    var resultType = typeof(TResponse).GetGenericArguments()[0];
                    var failureMethod = typeof(Result<>).MakeGenericType(resultType).GetMethod("Failure");
                    return (TResponse)failureMethod!.Invoke(null, new object[] { errors })!;
                }

                throw new ValidationException(failures);
            }
        }

        return await next();
    }
}
