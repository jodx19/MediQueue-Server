// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Common\ICommand.cs
using MediatR;

namespace MediQueue.Application.Common;

/// <summary>
/// Represents a command without a response value.
/// </summary>
public interface ICommand : IRequest<Result>
{
}

/// <summary>
/// Represents a command with a specific response type.
/// </summary>
public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}
