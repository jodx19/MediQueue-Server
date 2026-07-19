using MediQueue.Application.Interfaces;
using MediatR;

namespace MediQueue.Application.Auth.Commands.ResetPassword;

public sealed class ResetPasswordCommandHandler
    : IRequestHandler<ResetPasswordCommand>
{
    private readonly IAuthService _authService;

    public ResetPasswordCommandHandler(IAuthService authService)
        => _authService = authService;

    public async Task Handle(
        ResetPasswordCommand command,
        CancellationToken    ct)
    {
        await _authService.ResetPasswordAsync(
            command.Email,
            command.Token,
            command.NewPassword,
            ct);
    }
}
