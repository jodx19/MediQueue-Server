using System;
using System.Threading.Tasks;
using MediQueue.Domain.Enums;

namespace MediQueue.Application.Interfaces;

public interface IUsageValidatorService
{
    Task<bool> IsQuotaAvailableAsync(Guid tenantId, QuotaType quotaType);
}
