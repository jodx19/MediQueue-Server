using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediQueue.Application.Interfaces;
using MediQueue.Domain.Entities;
using MediQueue.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace MediQueue.Application.Common.Behaviors;

public sealed class AuditBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IAuditLogRepository  _auditRepo;
    private readonly ICurrentUserService  _currentUser;
    private readonly ITenantContext       _tenantContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    private static readonly HashSet<string> _trackedSuffixes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "Command"    
        };

    public AuditBehavior(
        IAuditLogRepository  auditRepo,
        ICurrentUserService  currentUser,
        ITenantContext       tenantContext,
        IHttpContextAccessor httpContextAccessor)
    {
        _auditRepo           = auditRepo;
        _currentUser         = currentUser;
        _tenantContext       = tenantContext;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<TResponse> Handle(
        TRequest                          request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken                 ct)
    {
        var requestName = typeof(TRequest).Name;

        bool shouldAudit = _trackedSuffixes
            .Any(suffix => requestName.EndsWith(
                suffix, StringComparison.OrdinalIgnoreCase));

        if (!shouldAudit)
            return await next();

        string? requestJson = null;
        try
        {
            requestJson = JsonSerializer.Serialize(request,
                new JsonSerializerOptions
                {
                    WriteIndented        = false,
                    ReferenceHandler     =
                        System.Text.Json.Serialization
                              .ReferenceHandler.IgnoreCycles,
                    DefaultIgnoreCondition =
                        System.Text.Json.Serialization
                              .JsonIgnoreCondition.WhenWritingNull
                });
        }
        catch {  }

        var  tenantId  = _tenantContext.TenantId;
        var  userId    = _currentUser.UserId;
        var  userEmail = _currentUser.Email ?? "anonymous";
        var  userRole  = _currentUser.Role  ?? "unknown";
        var  ip        = _httpContextAccessor
                            .HttpContext?
                            .Connection
                            .RemoteIpAddress?
                            .ToString();

        Guid?  entityId   = ExtractEntityId(request);
        string entityName = ExtractEntityName(requestName);
        string action     = ExtractAction(requestName);

        bool    succeeded    = true;
        string? errorMessage = null;
        string? responseJson = null;

        try
        {
            var response = await next();

            try
            {
                responseJson = response is null ? null
                    : JsonSerializer.Serialize(response,
                        new JsonSerializerOptions
                        {
                            WriteIndented    = false,
                            ReferenceHandler =
                                System.Text.Json.Serialization
                                      .ReferenceHandler.IgnoreCycles,
                        });
            }
            catch { /* silent */ }

            return response;
        }
        catch (Exception ex)
        {
            succeeded    = false;
            errorMessage = ex.Message;
            throw; 
        }
        finally
        {
            try
            {
                var log = AuditLog.Create(
                    tenantId:      tenantId,
                    userId:        userId,
                    userEmail:     userEmail,
                    userRole:      userRole,
                    action:        action,
                    entityName:    entityName,
                    entityId:      entityId,
                    requestName:   requestName,
                    succeeded:     succeeded,
                    oldValues:     requestJson,
                    newValues:     responseJson,
                    ipAddress:     ip,
                    errorMessage:  errorMessage);

                await _auditRepo.AddAsync(log, ct);
            }
            catch
            {
            }
        }
    }

    private static Guid? ExtractEntityId<T>(T request)
    {
        if (request is null) return null;
        try
        {
            var prop = typeof(T).GetProperty("Id")
                    ?? typeof(T).GetProperty("PatientId")
                    ?? typeof(T).GetProperty("AppointmentId")
                    ?? typeof(T).GetProperty("VisitId")
                    ?? typeof(T).GetProperty("InvoiceId")
                    ?? typeof(T).GetProperty("DoctorId");

            if (prop?.GetValue(request) is Guid id && id != Guid.Empty)
                return id;
        }
        catch { /* silent */ }
        return null;
    }

    private static string ExtractEntityName(string requestName)
    {
        var name = requestName
            .Replace("Command", "")
            .Replace("Create", "")
            .Replace("Update", "")
            .Replace("Delete", "")
            .Replace("Register", "")
            .Replace("Cancel",  "")
            .Replace("Finalize","")
            .Replace("Confirm", "")
            .Replace("Revoke",  "")
            .Trim();

        return string.IsNullOrEmpty(name) ? "Unknown" : name;
    }

    private static string ExtractAction(string requestName)
    {
        string[] knownActions =
        [
            "Create", "Update", "Delete", "Register",
            "Cancel", "Finalize", "Confirm", "Revoke",
            "Book",   "Start",   "Complete","Logout",
            "Login",  "Provision","Deactivate","Reactivate"
        ];

        foreach (var action in knownActions)
            if (requestName.StartsWith(action, StringComparison.OrdinalIgnoreCase))
                return action;

        return "Execute";
    }
}
