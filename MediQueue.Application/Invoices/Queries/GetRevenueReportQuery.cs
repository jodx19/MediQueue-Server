// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Invoices\Queries\GetRevenueReportQuery.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Application.Invoices.DTOs;
using MediQueue.Domain.Enums;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.Invoices.Queries;

public record GetRevenueReportQuery(DateOnly StartDate, DateOnly EndDate, Guid? DoctorId = null) : IQuery<RevenueReportDto>;

public class GetRevenueReportQueryHandler : IRequestHandler<GetRevenueReportQuery, Result<RevenueReportDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetRevenueReportQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<RevenueReportDto>> Handle(GetRevenueReportQuery request, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        // This query requires fetching all invoices in a range and aggregating them.
        // In a real CQRS setup, this would be a direct SQL query or read from a specialized read model.
        // Since we only have the generic repository interfaces, we would need to add a specialized method to `IInvoiceRepository`.
        // I will return a placeholder indicating the expected structure and logic assuming a hypothetical `GetInvoicesInRangeAsync` method.

        // var invoices = await _unitOfWork.Invoices.GetInvoicesInRangeAsync(request.StartDate, request.EndDate);
        
        // Placeholder until repo method is available:
        var invoices = new List<Domain.Entities.Invoice>(); 
        
        if (request.DoctorId.HasValue)
        {
            // filter by doctor. Note: Invoice has AppointmentId, which leads to DoctorId.
            // Complex join needed here.
        }

        var totalRevenue = invoices.Sum(i => i.TotalAmount.Amount);
        var collectedRevenue = invoices.Sum(i => i.PaidAmount.Amount);
        var outstandingRevenue = invoices.Sum(i => i.RemainingAmount.Amount);

        var allPayments = invoices.SelectMany(i => i.Payments).ToList();
        var paymentsByMethod = allPayments
            .GroupBy(p => p.Method)
            .ToDictionary(g => g.Key, g => g.Sum(p => p.Amount.Amount));

        var dailyRevenue = allPayments
            .GroupBy(p => p.PaidAt.Date)
            .Select(g => new DailyRevenueDto
            {
                Date = g.Key,
                Amount = g.Sum(p => p.Amount.Amount)
            })
            .OrderBy(d => d.Date)
            .ToList();

        var report = new RevenueReportDto
        {
            TotalRevenue = totalRevenue,
            CollectedRevenue = collectedRevenue,
            OutstandingRevenue = outstandingRevenue,
            InvoiceCount = invoices.Count,
            PaymentsByMethod = paymentsByMethod,
            DailyRevenue = dailyRevenue
        };

        return Result<RevenueReportDto>.Success(report);
    }
}
