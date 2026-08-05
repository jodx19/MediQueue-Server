// Path: MediQueue.Application/Reports/Queries/GetReportsQuery.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Application.Interfaces;

namespace MediQueue.Application.Reports.Queries;

// ──────────────────────────────────────────────────────────────────────────────
// Shared DTOs
// ──────────────────────────────────────────────────────────────────────────────

public sealed record ChartDataPoint(string Label, decimal Value);

public sealed class ReportsResponse
{
    public decimal TotalRevenue         { get; init; }
    public int     TotalAppointments    { get; init; }
    public int     TotalPatients        { get; init; }
    public int     NewPatientsThisMonth { get; init; }
    public int     TotalDoctors         { get; init; }
    public List<ChartDataPoint> ChartData { get; init; } = [];
}

// ──────────────────────────────────────────────────────────────────────────────
// C2-a: Revenue Report (delegates to existing IInvoiceRepository method)
// ──────────────────────────────────────────────────────────────────────────────

public record GetRevenueReportQuery(DateTime StartDate, DateTime EndDate)
    : IRequest<Result<ReportsResponse>>;

public sealed class GetRevenueReportQueryHandler
    : IRequestHandler<GetRevenueReportQuery, Result<ReportsResponse>>
{
    private readonly Domain.Interfaces.IUnitOfWork _unitOfWork;

    public GetRevenueReportQueryHandler(Domain.Interfaces.IUnitOfWork unitOfWork)
        => _unitOfWork = unitOfWork;

    public async Task<Result<ReportsResponse>> Handle(
        GetRevenueReportQuery request, CancellationToken ct)
    {
        var from = DateOnly.FromDateTime(request.StartDate);
        var to   = DateOnly.FromDateTime(request.EndDate);

        var revenueData = await _unitOfWork.Invoices.GetRevenueDataAsync(from, to);

        var chartData = revenueData
            .GroupBy(r => r.Date)
            .OrderBy(g => g.Key)
            .Select(g => new ChartDataPoint(
                g.Key.ToString("MMM dd"),
                g.Sum(x => x.TotalAmount)))
            .ToList();

        return Result<ReportsResponse>.Success(new ReportsResponse
        {
            TotalRevenue = chartData.Sum(p => p.Value),
            ChartData    = chartData
        });
    }
}

// ──────────────────────────────────────────────────────────────────────────────
// C2-b: Appointments by Status Report
// ──────────────────────────────────────────────────────────────────────────────

public record GetAppointmentsReportQuery(DateTime StartDate, DateTime EndDate)
    : IRequest<Result<ReportsResponse>>;

public sealed class GetAppointmentsReportQueryHandler
    : IRequestHandler<GetAppointmentsReportQuery, Result<ReportsResponse>>
{
    private readonly Domain.Interfaces.IUnitOfWork _unitOfWork;

    public GetAppointmentsReportQueryHandler(Domain.Interfaces.IUnitOfWork unitOfWork)
        => _unitOfWork = unitOfWork;

    public async Task<Result<ReportsResponse>> Handle(
        GetAppointmentsReportQuery request, CancellationToken ct)
    {
        // GetByDateRangeAsync is the existing repository method used by schedule queries
        var all = await _unitOfWork.Appointments.GetByDateRangeAsync(
            request.StartDate, request.EndDate, doctorId: null, ct);

        var chartData = all
            .GroupBy(a => a.Status.ToString())
            .Select(g => new ChartDataPoint(g.Key, g.Count()))
            .ToList();

        return Result<ReportsResponse>.Success(new ReportsResponse
        {
            TotalAppointments = all.Count,
            ChartData         = chartData
        });
    }
}

// ──────────────────────────────────────────────────────────────────────────────
// C2-c: Patients Summary Report
// ──────────────────────────────────────────────────────────────────────────────

public record GetPatientsReportQuery : IRequest<Result<ReportsResponse>>;

public sealed class GetPatientsReportQueryHandler
    : IRequestHandler<GetPatientsReportQuery, Result<ReportsResponse>>
{
    private readonly Domain.Interfaces.IUnitOfWork _unitOfWork;

    public GetPatientsReportQueryHandler(Domain.Interfaces.IUnitOfWork unitOfWork)
        => _unitOfWork = unitOfWork;

    public async Task<Result<ReportsResponse>> Handle(
        GetPatientsReportQuery request, CancellationToken ct)
    {
        // Use the count method that already exists in the repository
        var total = await _unitOfWork.Patients.CountAsync();

        // Fetch first page with a large size to get all for the month-breakdown chart.
        // For small-to-medium clinics this is fine; a dedicated DB query would be
        // added later if the patient count grows significantly.
        var paged = await _unitOfWork.Patients.SearchAsync(string.Empty, 1, 10_000);
        var now   = DateTime.UtcNow;
        var newThisMonth = paged.Items.Count(p =>
            p.CreatedAt.Year == now.Year && p.CreatedAt.Month == now.Month);

        // Gender breakdown chart
        var chartData = paged.Items
            .GroupBy(p => p.Gender.ToString())
            .Select(g => new ChartDataPoint(g.Key, g.Count()))
            .ToList();

        return Result<ReportsResponse>.Success(new ReportsResponse
        {
            TotalPatients        = total,
            NewPatientsThisMonth = newThisMonth,
            ChartData            = chartData
        });
    }
}

// ──────────────────────────────────────────────────────────────────────────────
// C2-d: Doctors by Specialty Report
// ──────────────────────────────────────────────────────────────────────────────

public record GetDoctorsReportQuery : IRequest<Result<ReportsResponse>>;

public sealed class GetDoctorsReportQueryHandler
    : IRequestHandler<GetDoctorsReportQuery, Result<ReportsResponse>>
{
    private readonly Domain.Interfaces.IUnitOfWork _unitOfWork;

    public GetDoctorsReportQueryHandler(Domain.Interfaces.IUnitOfWork unitOfWork)
        => _unitOfWork = unitOfWork;

    public async Task<Result<ReportsResponse>> Handle(
        GetDoctorsReportQuery request, CancellationToken ct)
    {
        var total = await _unitOfWork.Doctors.CountAsync();

        // Fetch all doctors (page 1, large size) for specialty grouping
        var paged = await _unitOfWork.Doctors.GetAllAsync(1, 10_000);

        var chartData = paged.Items
            .GroupBy(d => d.Specialty.ToString())
            .Select(g => new ChartDataPoint(g.Key, g.Count()))
            .ToList();

        return Result<ReportsResponse>.Success(new ReportsResponse
        {
            TotalDoctors = total,
            ChartData    = chartData
        });
    }
}
