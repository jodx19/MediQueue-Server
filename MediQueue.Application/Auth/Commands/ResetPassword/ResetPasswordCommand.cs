using MediatR;

namespace MediQueue.Application.Auth.Commands.ResetPassword;

public record ResetPasswordCommand(
    string Email,
    string Token,
    string NewPassword,
    string ConfirmPassword)
    : IRequest;
