using Microsoft.EntityFrameworkCore;
using MediQueue.Domain.Entities;
using MediQueue.Domain.Enums;
using MediQueue.Infrastructure.Persistence.Context;

namespace MediQueue.Infrastructure.ExternalServices;

public class InvoiceOverdueJob
{
    private readonly ClinicDbContext _context;

    public InvoiceOverdueJob(ClinicDbContext context)
    {
        _context = context;
    }

    public async Task ExecuteAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        
        var overdueInvoices = await _context.Invoices
            .Where(i => i.Status == InvoiceStatus.Issued || i.Status == InvoiceStatus.PartiallyPaid)
            .Where(i => i.DueDate < today)
            .ToListAsync();

        foreach (var invoice in overdueInvoices)
        {
            // Update status using reflection if private setter, or direct assignment
            typeof(Invoice).GetProperty("Status")?.SetValue(invoice, InvoiceStatus.Overdue);
        }

        if (overdueInvoices.Any())
        {
            await _context.SaveChangesAsync();
        }
    }
}
