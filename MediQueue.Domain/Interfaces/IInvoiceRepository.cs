// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Domain\Interfaces\IInvoiceRepository.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediQueue.Domain.Common;
using MediQueue.Domain.Entities;
using MediQueue.Domain.Enums;

namespace MediQueue.Domain.Interfaces;

/// <summary>
/// Represents a revenue data record grouped for reporting.
/// </summary>
public class RevenueDataDto
{
    public DateOnly Date { get; set; }
    public PaymentMethod Method { get; set; }
    public decimal TotalAmount { get; set; }
    public int PaymentCount { get; set; }
}

public interface IInvoiceRepository
{
    Task<Invoice?> GetByIdAsync(Guid id);
    Task<PagedResult<Invoice>> GetByPatientAsync(Guid patientId, int page, int size);

    /// <summary>Clinic-wide invoice list with optional filters, newest first.</summary>
    Task<PagedResult<Invoice>> GetPagedAsync(
        string? status,
        DateTime? from,
        DateTime? to,
        int page,
        int size,
        CancellationToken cancellationToken = default);
    Task<Invoice?> GetByAppointmentIdAsync(Guid appointmentId);
    Task AddAsync(Invoice invoice);
    Task UpdateAsync(Invoice invoice);

    /// <summary>
    /// Returns payments grouped by date and payment method within the given date range.
    /// Used by GetRevenueReportQuery.
    /// </summary>
    Task<List<RevenueDataDto>> GetRevenueDataAsync(DateOnly from, DateOnly to);

    Task<int> CountByStatusAsync(InvoiceStatus status);
    Task<decimal> GetRevenueInRangeAsync(DateTime from, DateTime to);
    Task<List<Invoice>> GetOverdueInvoicesAsync(DateOnly threshold);
}
