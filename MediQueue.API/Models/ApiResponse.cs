// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.API\Models\ApiResponse.cs
using System.Collections.Generic;

namespace MediQueue.API.Models;

/// <summary>
/// A unified response wrapper for all API endpoints.
/// </summary>
/// <typeparam name="T">The type of the data being returned.</typeparam>
public class ApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }

    public ApiResponse() { }

    public ApiResponse(bool isSuccess, T? data = default, string? message = null, List<string>? errors = null)
    {
        IsSuccess = isSuccess;
        Data = data;
        Message = message;
        Errors = errors;
    }

    public static ApiResponse<T> Success(T data, string? message = null) 
        => new(true, data, message);

    public static ApiResponse<T> Failure(List<string> errors, string? message = null) 
        => new(false, default, message, errors);
    
    public static ApiResponse<T> Failure(string error, string? message = null) 
        => new(false, default, message, new List<string> { error });
}
