using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using MediQueue.Application.Common;
using MediQueue.Application.Interfaces;
using MediQueue.Domain.Entities;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that intercepts all Commands to log audit trails.
/// </summary>
public class AuditLogBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<AuditLogBehavior<TRequest, TResponse>> _logger;
    private readonly ICurrentUserService _currentUserService;
    private readonly ITenantContext _tenantContext;
    private readonly IUnitOfWork _unitOfWork;

    public AuditLogBehavior(
        ILogger<AuditLogBehavior<TRequest, TResponse>> logger,
        ICurrentUserService currentUserService,
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _currentUserService = currentUserService;
        _tenantContext = tenantContext;
        _unitOfWork = unitOfWork;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Only audit commands, not queries. Commands typically implement ICommand or ICommand<T>
        // But checking the namespace or interface is safer. We'll check if it implements ICommand.
        bool isCommand = request is ICommand || request.GetType().GetInterface(typeof(ICommand<>).Name) != null;

        if (!isCommand)
        {
            return await next();
        }

        var requestName = typeof(TRequest).Name;
        var tenantId = _tenantContext.TenantId != Guid.Empty ? _tenantContext.TenantId : _currentUserService.TenantId;
        var userId = _currentUserService.UserId;
        var userEmail = _currentUserService.Email;
        var userRole = _currentUserService.Role;

        TResponse response;
        bool isSuccess = true;
        string? errorMessage = null;

        try
        {
            response = await next();
            
            // Check if it's a Result pattern and failed
            if (response is Result result && result.IsFailure)
            {
                isSuccess = false;
                errorMessage = result.Error;
            }
        }
        catch (Exception ex)
        {
            isSuccess = false;
            errorMessage = ex.Message;
            
            // Save audit log before re-throwing
            await SaveAuditLogSafeAsync(tenantId, userId, userEmail, userRole, requestName, isSuccess, errorMessage, cancellationToken);
            throw;
        }

        // Save audit log on normal completion
        await SaveAuditLogSafeAsync(tenantId, userId, userEmail, userRole, requestName, isSuccess, errorMessage, cancellationToken);
        
        return response;
    }

    private async Task SaveAuditLogSafeAsync(
        Guid tenantId,
        Guid? userId,
        string? userEmail,
        string? userRole,
        string action,
        bool isSuccess,
        string? errorMessage,
        CancellationToken cancellationToken)
    {
        try
        {
            // If there's no tenant, we can't properly partition the audit log.
            // This might happen on anonymous calls like LoginCommand before tenant resolution.
            if (tenantId == Guid.Empty)
            {
                return;
            }

            var auditLog = AuditLog.Create(
                tenantId,
                userId,
                userEmail,
                userRole,
                action,
                isSuccess,
                errorMessage);

            await _unitOfWork.AuditLogs.AddAsync(auditLog, cancellationToken);
            
            // We save changes independently for the audit log so it commits even if the main transaction rolled back.
            // Wait, if we use the same UnitOfWork, it will commit the whole thing.
            // To be safe and not affect the handler's transaction, we just call SaveChangesAsync. 
            // If the handler succeeded, this saves the audit + the handler's changes.
            // If the handler failed (e.g. validation), there are no domain changes to save anyway.
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            // Do not throw! Audit logging failure should not break the application.
            _logger.LogError(ex, "Failed to save audit log for action {Action}", action);
        }
    }
}
