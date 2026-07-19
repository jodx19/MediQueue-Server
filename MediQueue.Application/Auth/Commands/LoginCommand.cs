using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using MediQueue.Application.Auth.DTOs;
using MediQueue.Application.Common;
using MediQueue.Application.Interfaces;

namespace MediQueue.Application.Auth.Commands;

public class LoginCommand : IRequest<Result<AuthResponseDto>>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponseDto>>
{
    private readonly IAuthService _authService;
    private readonly ICacheService _cacheService;

    public LoginCommandHandler(IAuthService authService, ICacheService cacheService)
    {
        _authService = authService;
        _cacheService = cacheService;
    }

    public async Task<Result<AuthResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var cacheKey = $"staff-login-attempts:{request.Email.Trim().ToLowerInvariant()}";
        var attempts = await _cacheService.GetAsync<int>(cacheKey);

        if (attempts >= 5)
        {
            return Result<AuthResponseDto>.Failure("Account temporarily locked. Try again in 15 minutes.");
        }

        var result = await _authService.LoginAsync(new LoginRequestDto(request.Email, request.Password));

        if (!result.IsSuccess)
        {
            await _cacheService.SetAsync(cacheKey, attempts + 1, TimeSpan.FromMinutes(15));
            return Result<AuthResponseDto>.Failure("Invalid email or password.");
        }

        await _cacheService.RemoveAsync(cacheKey);

        return result;
    }
}
