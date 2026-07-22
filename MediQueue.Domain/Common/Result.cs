namespace MediQueue.Domain.Common;

/// <summary>
/// Lightweight operation result surfaced from Domain entities. Mirrors the
/// shape of <c>MediQueue.Application.Common.Result</c> but lives in the Domain
/// layer so that entities can return typed outcomes without depending on the
/// Application project (which would invert the dependency direction).
/// Application handlers adapt this to <c>Application.Common.Result</c>.
/// </summary>
public sealed class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }

    private Result(bool isSuccess, string? error)
    {
        IsSuccess = isSuccess;
        Error     = error;
    }

    public static Result Success()            => new(true,  null);
    public static Result Failure(string error) => new(false, error);
}
