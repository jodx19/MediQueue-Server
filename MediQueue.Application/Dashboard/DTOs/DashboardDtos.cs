// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Dashboard\DTOs\DashboardDtos.cs
using System;
using System.Collections.Generic;

namespace MediQueue.Application.Dashboard.DTOs;

public record ClinicStatsDto(
    int TotalPatients,
    int TotalDoctors,
    int AppointmentsToday,
    int PendingInvoices,
    decimal RevenueMonthToDate);

public record RevenueReportDto(
    DateTime StartDate,
    DateTime EndDate,
    decimal TotalRevenue,
    List<DailyRevenueDto> DailyRevenue);

public record DailyRevenueDto(DateTime Date, decimal Amount);
