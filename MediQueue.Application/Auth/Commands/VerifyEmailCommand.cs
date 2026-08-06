using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.Auth.Commands;

public class VerifyEmailCommand : IRequest<Result<bool>>
{
    public string UserId { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}

public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public VerifyEmailCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.UserId, out var userId))
            return Result<bool>.Failure("Invalid user ID format.");

        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
            return Result<bool>.Failure("User not found.");

        if (user.EmailConfirmed)
            return Result<bool>.Success(true); // Already verified — idempotent

        var confirmed = user.ConfirmEmailVerification(request.Token);
        if (!confirmed)
            return Result<bool>.Failure("Invalid or expired verification token. Please request a new verification email.");

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
