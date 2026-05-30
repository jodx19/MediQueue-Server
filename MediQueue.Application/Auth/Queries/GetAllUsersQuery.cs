using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.Auth.Queries;

public record UserListItemDto(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    bool IsActive,
    DateTime CreatedAt
);

public record GetAllUsersQuery : IRequest<Result<IEnumerable<UserListItemDto>>>;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, Result<IEnumerable<UserListItemDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllUsersQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<UserListItemDto>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _unitOfWork.Users.GetAllAsync();
        var dtos = users.Select(u => new UserListItemDto(
            u.Id.ToString(),
            u.Email,
            u.FirstName,
            u.LastName,
            u.Role.ToString(),
            u.IsActive,
            u.CreatedAt
        ));
        return Result<IEnumerable<UserListItemDto>>.Success(dtos);
    }
}
