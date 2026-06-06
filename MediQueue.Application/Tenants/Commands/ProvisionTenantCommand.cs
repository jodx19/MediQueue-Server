using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using MediQueue.Application.Common;
using MediQueue.Application.Interfaces;
using MediQueue.Domain.Entities;
using MediQueue.Domain.Enums;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.Tenants.Commands;

/// <summary>
/// Creates a new tenant with:
/// - Tenant record
/// - Default admin user
/// </summary>
public record ProvisionTenantCommand(
    string ClinicName,
    string Subdomain,
    string AdminEmail,
    string AdminPassword,
    string AdminFirstName,
    string AdminLastName,
    TenantPlan Plan = TenantPlan.Basic
) : IRequest<Result<ProvisionTenantResult>>;

public record ProvisionTenantResult(
    Guid TenantId,
    string Subdomain,
    string AdminEmail,
    string PortalUrl
);

public class ProvisionTenantCommandValidator : AbstractValidator<ProvisionTenantCommand>
{
    public ProvisionTenantCommandValidator(ITenantRepository tenantRepo)
    {
        RuleFor(x => x.ClinicName)
            .NotEmpty().MaximumLength(200);

        RuleFor(x => x.Subdomain)
            .NotEmpty()
            .MaximumLength(50)
            .Matches(@"^[a-z0-9][a-z0-9-]{2,48}[a-z0-9]$")
            .WithMessage("Subdomain must be 4-50 lowercase letters, numbers, or hyphens.")
            .MustAsync(async (sub, ct) => !await tenantRepo.SubdomainExistsAsync(sub, ct))
            .WithMessage("This subdomain is already taken.");

        RuleFor(x => x.AdminEmail)
            .NotEmpty().EmailAddress();

        RuleFor(x => x.AdminPassword)
            .NotEmpty().MinimumLength(8)
            .Matches(@"[A-Z]").WithMessage("Needs uppercase.")
            .Matches(@"[0-9]").WithMessage("Needs number.");
    }
}

public class ProvisionTenantCommandHandler : IRequestHandler<ProvisionTenantCommand, Result<ProvisionTenantResult>>
{
    private readonly ITenantRepository _tenantRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher<AppUser> _passwordHasher;

    public ProvisionTenantCommandHandler(
        ITenantRepository tenantRepo,
        IUnitOfWork unitOfWork,
        IPasswordHasher<AppUser> passwordHasher)
    {
        _tenantRepo = tenantRepo;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<ProvisionTenantResult>> Handle(
        ProvisionTenantCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Create tenant
        var tenant = Tenant.Create(
            request.ClinicName,
            request.Subdomain,
            request.AdminEmail,
            request.Plan);

        await _tenantRepo.AddAsync(tenant, cancellationToken);

        // 2. Create admin user for this tenant
        var adminUser = AppUser.Create(
            username: request.AdminEmail,
            email: request.AdminEmail,
            firstName: request.AdminFirstName,
            lastName: request.AdminLastName,
            phoneNumber: string.Empty,
            passwordHash: string.Empty, // Will set below
            role: UserRole.Admin,
            doctorId: null,
            patientId: null);

        var passwordHash = _passwordHasher.HashPassword(adminUser, request.AdminPassword);
        adminUser.SetPasswordHash(passwordHash);

        // Stamp TenantId manually (user created outside normal request context)
        adminUser.TenantId = tenant.Id;

        await _unitOfWork.Users.AddAsync(adminUser);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var result = new ProvisionTenantResult(
            tenant.Id,
            tenant.Subdomain,
            tenant.AdminEmail,
            $"https://{tenant.Subdomain}.mediqueue.com"
        );

        return Result<ProvisionTenantResult>.Success(result);
    }
}
