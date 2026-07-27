using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Domain.Entities;
using MediQueue.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace MediQueue.Application.Auth.Commands;

public class ResetPasswordCommand : ICommand
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8)
            .WithMessage("Password must be at least 8 characters long.");
    }
}

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher<AppUser> _passwordHasher;

    public ResetPasswordCommandHandler(IUnitOfWork unitOfWork, IPasswordHasher<AppUser> passwordHasher)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);
        
        if (user == null || 
            user.PasswordResetToken != request.Token || 
            user.PasswordResetTokenExpiresAt == null || 
            user.PasswordResetTokenExpiresAt < DateTime.UtcNow)
        {
            return Result.Failure("Invalid or expired password reset token.");
        }

        var newPasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
        
        user.ResetPassword(newPasswordHash);

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
