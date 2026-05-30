using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using MediQueue.Application.Auth.DTOs;
using MediQueue.Application.Common;
using MediQueue.Application.Interfaces;

namespace MediQueue.Application.Auth.Commands.PatientLogin;

public record PatientLoginCommand(
    string Mrn,
    DateTime DateOfBirth
) : IRequest<Result<AuthResponseDto>>;

public class PatientLoginCommandValidator : AbstractValidator<PatientLoginCommand>
{
    public PatientLoginCommandValidator()
    {
        RuleFor(x => x.Mrn).NotEmpty().Matches(@"^MRN-\d{8}-[A-Z0-9]{4}$");
        RuleFor(x => x.DateOfBirth).NotEmpty().LessThan(DateTime.UtcNow);
    }
}

public class PatientLoginCommandHandler : IRequestHandler<PatientLoginCommand, Result<AuthResponseDto>>
{
    private readonly IAuthService _authService;

    public PatientLoginCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<Result<AuthResponseDto>> Handle(PatientLoginCommand request, CancellationToken cancellationToken)
    {
        return await _authService.PatientLoginAsync(request.Mrn, request.DateOfBirth);
    }
}
