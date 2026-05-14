using System;

namespace MediQueue.Application.Invoices.DTOs;

/// <summary>Lightweight row for clinic-wide invoice listing.</summary>
public class InvoiceListItemDto
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    /// <summary>UTC issue timestamp (maps from invoice IssuedAt).</summary>
    public DateTime CreatedAt { get; set; }
}
