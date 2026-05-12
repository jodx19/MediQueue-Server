using System;
using System.Collections.Generic;

namespace MediQueue.Domain.Common;

public class Result<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }
    public int StatusCode { get; set; } = 200;

    public static Result<T> Success(T data, string? message = null) 
        => new Result<T> { IsSuccess = true, Data = data, Message = message, StatusCode = 200 };

    public static Result<T> Failure(List<string> errors, string? message = null, int statusCode = 400) 
        => new Result<T> { IsSuccess = false, Errors = errors, Message = message, StatusCode = statusCode };

    public static Result<T> Failure(string error, string? message = null, int statusCode = 400) 
        => new Result<T> { IsSuccess = false, Errors = new List<string> { error }, Message = message, StatusCode = statusCode };
}
