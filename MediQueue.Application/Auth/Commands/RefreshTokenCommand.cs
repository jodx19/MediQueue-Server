// Path: MediQueue.Application/Auth/Commands/RefreshTokenCommand.cs
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MediQueue.Application.Auth.DTOs;
using MediQueue.Application.Common;
using MediQueue.Application.Interfaces;

namespace MediQueue.Application.Auth.Commands;

public class RefreshTokenCommand : IRequest<Result<AuthResponseDto>>
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthResponseDto>>
{
    private readonly IAuthService _authService;

    public RefreshTokenCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<Result<AuthResponseDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        return await _authService.RefreshTokenAsync(new RefreshTokenRequestDto(request.Token, request.RefreshToken));
    }
}
