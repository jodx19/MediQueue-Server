// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Common\Result.cs
using System;

namespace MediQueue.Application.Common;

/// <summary>
/// Represents the result of an operation.
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }

    protected Result(bool isSuccess, string? error)
    {
        if (isSuccess && error != null)
            throw new InvalidOperationException("Success cannot have an error.");
        if (!isSuccess && string.IsNullOrWhiteSpace(error))
            throw new InvalidOperationException("Failure must have an error.");

        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new Result(true, null);
    public static Result Failure(string error) => new Result(false, error);
}
