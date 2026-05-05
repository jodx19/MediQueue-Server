// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Dashboard\Queries\GetRevenueReportQuery.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Application.Dashboard.DTOs;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.Dashboard.Queries;

public record GetRevenueReportQuery(DateTime StartDate, DateTime EndDate) : IQuery<RevenueReportDto>;

public class GetRevenueReportQueryHandler : IRequestHandler<GetRevenueReportQuery, Result<RevenueReportDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetRevenueReportQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<RevenueReportDto>> Handle(GetRevenueReportQuery request, CancellationToken cancellationToken)
    {
        var from = DateOnly.FromDateTime(request.StartDate);
        var to = DateOnly.FromDateTime(request.EndDate);
        
        var revenueData = await _unitOfWork.Invoices.GetRevenueDataAsync(from, to);
        
        var dailyRevenue = revenueData
            .GroupBy(r => r.Date)
            .Select(g => new DailyRevenueDto(
                g.Key.ToDateTime(TimeOnly.MinValue), 
                g.Sum(x => x.TotalAmount)))
            .OrderBy(d => d.Date)
            .ToList();

        var totalRevenue = dailyRevenue.Sum(d => d.Amount);

        var report = new RevenueReportDto(
            request.StartDate,
            request.EndDate,
            totalRevenue,
            dailyRevenue);

        return Result<RevenueReportDto>.Success(report);
    }
}
