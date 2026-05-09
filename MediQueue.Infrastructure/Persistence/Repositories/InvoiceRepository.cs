// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Infrastructure\Persistence\Repositories\InvoiceRepository.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediQueue.Domain.Common;
using MediQueue.Domain.Entities;
using MediQueue.Domain.Interfaces;
using MediQueue.Infrastructure.Persistence.Context;

namespace MediQueue.Infrastructure.Persistence.Repositories;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly ClinicDbContext _context;

    public InvoiceRepository(ClinicDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets an invoice by ID.
    /// NOTE: Invoice has no navigation properties — only FK Guids (PatientId, AppointmentId).
    /// </summary>
    public async Task<Invoice?> GetByIdAsync(Guid id)
    {
        return await _context.Invoices
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    /// <summary>Returns paginated invoices for a patient, ordered by most recent first.</summary>
    public async Task<PagedResult<Invoice>> GetByPatientAsync(Guid patientId, int page, int size)
    {
        var query = _context.Invoices
            .AsNoTracking()
            .Where(i => i.PatientId == patientId)
            .OrderByDescending(i => i.IssuedAt); // FIXED: was i.IssueDate (non-existent)

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        return new PagedResult<Invoice>(items, total, page, size);
    }

    public async Task<Invoice?> GetByAppointmentIdAsync(Guid appointmentId)
    {
        return await _context.Invoices
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.AppointmentId == appointmentId);
    }

    public async Task AddAsync(Invoice invoice)
    {
        await _context.Invoices.AddAsync(invoice);
    }

    public async Task UpdateAsync(Invoice invoice)
    {
        _context.Invoices.Update(invoice);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Returns revenue data grouped by date and payment method within the date range.
    /// Used to power the GetRevenueReportQuery.
    /// </summary>
    public async Task<List<RevenueDataDto>> GetRevenueDataAsync(DateOnly from, DateOnly to)
    {
        // Load payments within the date range, then group in memory
        // (EF Core 8 has limited support for DateOnly comparisons in SQL — safer to filter then group)
        var fromDate = from.ToDateTime(TimeOnly.MinValue);
        var toDate = to.ToDateTime(TimeOnly.MaxValue);

        var payments = await _context.Invoices
            .AsNoTracking()
            .Where(i => i.IssuedAt >= fromDate && i.IssuedAt <= toDate)
            .SelectMany(i => i.Payments.Select(p => new
            {
                Date = DateOnly.FromDateTime(p.PaidAt),
                p.Method,
                p.Amount.Amount
            }))
            .ToListAsync();

        return payments
            .GroupBy(p => new { p.Date, p.Method })
            .Select(g => new RevenueDataDto
            {
                Date = g.Key.Date,
                Method = g.Key.Method,
                TotalAmount = g.Sum(x => x.Amount),
                PaymentCount = g.Count()
            })
            .OrderBy(r => r.Date)
            .ThenBy(r => r.Method)
            .ToList();
    }

    public async Task<int> CountByStatusAsync(Domain.Enums.InvoiceStatus status)
    {
        return await _context.Invoices
            .AsNoTracking()
            .CountAsync(i => i.Status == status);
    }

    public async Task<decimal> GetRevenueInRangeAsync(DateTime from, DateTime to)
    {
        var payments = await _context.Invoices
            .AsNoTracking()
            .Where(i => i.IssuedAt >= from && i.IssuedAt <= to)
            .SelectMany(i => i.Payments)
            .ToListAsync();

        return payments.Sum(p => p.Amount.Amount);
    }

    public async Task<List<Invoice>> GetOverdueInvoicesAsync(DateOnly threshold)
    {
        return await _context.Invoices
            .Where(i => i.Status == Domain.Enums.InvoiceStatus.Issued || i.Status == Domain.Enums.InvoiceStatus.PartiallyPaid)
            .Where(i => i.DueDate < threshold)
            .ToListAsync();
    }
}
