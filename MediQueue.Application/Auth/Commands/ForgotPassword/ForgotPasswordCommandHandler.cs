using MediQueue.Application.Interfaces;
using MediatR;

namespace MediQueue.Application.Auth.Commands.ForgotPassword;

public sealed class ForgotPasswordCommandHandler
    : IRequestHandler<ForgotPasswordCommand>
{
    private readonly IAuthService _authService;

    public ForgotPasswordCommandHandler(IAuthService authService)
        => _authService = authService;

    public async Task Handle(
        ForgotPasswordCommand command,
        CancellationToken     ct)
    {
        // Never reveal whether the email exists (anti-enumeration).
        await _authService.ForgotPasswordAsync(command.Email, ct);
    }
}
