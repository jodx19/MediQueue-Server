using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Application.Interfaces;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.Auth.Commands;

public class ForgotPasswordCommand : ICommand
{
    public string Email { get; set; } = string.Empty;
}

public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email is required.");
    }
}

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;

    public ForgotPasswordCommandHandler(IUnitOfWork unitOfWork, IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
    }

    public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);
        if (user == null)
        {
            // Silently return success to prevent email enumeration
            return Result.Success();
        }

        // Generate a cryptographically secure 6-digit PIN
        var pin = RandomNumberGenerator.GetInt32(100000, 999999).ToString();

        user.GeneratePasswordResetToken(pin, TimeSpan.FromHours(1));

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var body = $@"
            <h2>Password Reset Request</h2>
            <p>Hello {user.FirstName},</p>
            <p>We received a request to reset your password. Use the following PIN code to reset it:</p>
            <h3 style='background-color:#f4f4f4;padding:10px;display:inline-block;border-radius:5px;'>{pin}</h3>
            <p>This code will expire in 1 hour.</p>
            <p>If you did not request this, please ignore this email.</p>
        ";

        // We ignore failures from the email service so we don't crash the request
        try
        {
            await _emailService.SendEmailAsync(user.Email, "MediQueue - Password Reset PIN", body);
        }
        catch
        {
            // Log failure in a real app, but for security reasons don't return an error 
            // that reveals the email was real but failed to send.
        }

        return Result.Success();
    }
}
