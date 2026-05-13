// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Invoices\DTOs\InvoiceDtos.cs
using System;
using System.Collections.Generic;
using MediQueue.Domain.Enums;

namespace MediQueue.Application.Invoices.DTOs;

public class InvoiceItemDto
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

public class PaymentDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public DateTime PaidAt { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
}

public class InvoiceDto
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public Guid? AppointmentId { get; set; }
    public DateTime IssuedAt { get; set; }
    public DateOnly DueDate { get; set; }
    public InvoiceStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
}

public class InvoiceDetailDto : InvoiceDto
{
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    
    public List<InvoiceItemDto> Items { get; set; } = [];
    public List<PaymentDto> Payments { get; set; } = [];
}

public class InvoiceSummaryDto : InvoiceDto
{
}

public class RevenueReportDto
{
    public decimal TotalRevenue { get; set; }
    public decimal CollectedRevenue { get; set; }
    public decimal OutstandingRevenue { get; set; }
    public int InvoiceCount { get; set; }
    public Dictionary<PaymentMethod, decimal> PaymentsByMethod { get; set; } = [];
    public List<DailyRevenueDto> DailyRevenue { get; set; } = [];
}

public class DailyRevenueDto
{
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
}
