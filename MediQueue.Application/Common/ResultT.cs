// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Common\ResultT.cs
using System;

namespace MediQueue.Application.Common;

/// <summary>
/// Represents the result of an operation with a return value.
/// </summary>
public class Result<T> : Result
{
    private readonly T? _value;

    public T? Value => IsSuccess 
        ? _value 
        : throw new InvalidOperationException("The value of a failure result can not be accessed.");

    protected Result(bool isSuccess, string? error, T? value) 
        : base(isSuccess, error)
    {
        _value = value;
    }

    public static Result<T> Success(T value) => new Result<T>(true, null, value);
    public static new Result<T> Failure(string error) => new Result<T>(false, error, default);
}
