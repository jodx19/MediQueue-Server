// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.API\Middleware\ApiResponseFilter.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MediQueue.API.Models;
using System.Linq;

namespace MediQueue.API.Middleware;

/// <summary>
/// A global filter that wraps all successful ObjectResults into a unified ApiResponse wrapper.
/// This ensures consistency even if a controller returns Ok(data) directly.
/// </summary>
public class ApiResponseFilter : IResultFilter
{
    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.Result is ObjectResult objectResult)
        {
            // If it's already an ApiResponse, don't wrap it again
            if (objectResult.Value != null && objectResult.Value.GetType().IsGenericType &&
                objectResult.Value.GetType().GetGenericTypeDefinition() == typeof(ApiResponse<>))
            {
                return;
            }

            var statusCode = objectResult.StatusCode ?? 200;
            var isSuccess = statusCode >= 200 && statusCode < 300;

            var responseType = typeof(ApiResponse<>).MakeGenericType(objectResult.Value?.GetType() ?? typeof(object));
            
            object? wrappedResult;
            if (isSuccess)
            {
                wrappedResult = System.Activator.CreateInstance(responseType, true, objectResult.Value, null, null);
            }
            else
            {
                // For error results, we try to extract the error message
                string? message = "An error occurred.";
                List<string>? errors = null;

                if (objectResult.Value is string errorStr)
                {
                    errors = new List<string> { errorStr };
                }
                else if (objectResult.Value is IDictionary<string, string[]> validationErrors)
                {
                    message = "Validation failed.";
                    errors = validationErrors.SelectMany(x => x.Value).ToList();
                }
                else if (objectResult.Value is SerializableError serializableError)
                {
                    message = "Validation failed.";
                    errors = serializableError.SelectMany(x => (string[])x.Value).ToList();
                }
                else
                {
                    message = objectResult.Value?.ToString() ?? message;
                }

                wrappedResult = System.Activator.CreateInstance(responseType, false, default, message, errors);
            }

            objectResult.Value = wrappedResult;
        }
    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
        // No action needed after execution
    }
}
