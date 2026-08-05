// Path: MediQueue.Application/Auth/Commands/VerifyEmailCommand.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.Auth.Commands;

public class VerifyEmailCommand : IRequest<Result>
{
    public string UserId { get; set; } = null!;
    public string VerificationToken { get; set; } = null!;
}

public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public VerifyEmailCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.UserId, out var userId))
            return Result.Failure("Invalid user identifier.");

        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user is null)
            return Result.Failure("User not found.");

        // Delegate token validation & state change to the domain entity
        var domainResult = user.ConfirmEmail(request.VerificationToken);
        if (!domainResult.IsSuccess)
            return Result.Failure(domainResult.Error!);

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
