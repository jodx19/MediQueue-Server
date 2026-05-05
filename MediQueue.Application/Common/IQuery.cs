// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Common\IQuery.cs
using MediatR;

namespace MediQueue.Application.Common;

/// <summary>
/// Represents a query with a specific response type.
/// </summary>
public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}
