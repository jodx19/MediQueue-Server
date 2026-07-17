// Path: MediQueue.Application/Auth/Commands/LogoutCommand.cs
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MediQueue.Application.Interfaces;

namespace MediQueue.Application.Auth.Commands;

/// <summary>
/// Revokes the current user's refresh token. The user is identified from the
/// authenticated request's <see cref="ICurrentUserService.UserId"/>; the
/// command itself carries no payload by design so a leaked access token can
/// be used to log out only the very account it belongs to — no other.
/// </summary>
public sealed record LogoutCommand : IRequest;

public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand>
{
    private readonly IAuthService _authService;
    private readonly ICurrentUserService _currentUser;

    public LogoutCommandHandler(
        IAuthService authService,
        ICurrentUserService currentUser)
    {
        _authService = authService;
        _currentUser = currentUser;
    }

    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        // ICurrentUserService.UserId is Guid? — silently no-op when there is no
        // authenticated user (defense in depth; the endpoint is [Authorize]).
        var userId = _currentUser.UserId;
        if (!userId.HasValue)
            return;

        await _authService.LogoutAsync(userId.Value.ToString(), cancellationToken);
    }
}
