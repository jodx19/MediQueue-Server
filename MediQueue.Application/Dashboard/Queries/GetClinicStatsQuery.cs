// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Dashboard\Queries\GetClinicStatsQuery.cs
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Application.Dashboard.DTOs;
using MediQueue.Domain.Interfaces;
using MediQueue.Domain.Enums;

namespace MediQueue.Application.Dashboard.Queries;

public record GetClinicStatsQuery : IQuery<ClinicStatsDto>;

public class GetClinicStatsQueryHandler : IRequestHandler<GetClinicStatsQuery, Result<ClinicStatsDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetClinicStatsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ClinicStatsDto>> Handle(GetClinicStatsQuery request, CancellationToken cancellationToken)
    {
        var totalPatients = await _unitOfWork.Patients.CountAsync();
        var totalDoctors = await _unitOfWork.Doctors.CountAsync();
        
        var today = DateTime.UtcNow.Date;
        var appointmentsToday = await _unitOfWork.Appointments.CountByDateAsync(today);
        
        var pendingInvoices = await _unitOfWork.Invoices.CountByStatusAsync(InvoiceStatus.Issued);
        
        var startOfMonth = new DateTime(today.Year, today.Month, 1);
        var revenueMonth = await _unitOfWork.Invoices.GetRevenueInRangeAsync(startOfMonth, today.AddDays(1));

        var stats = new ClinicStatsDto(
            totalPatients,
            totalDoctors,
            appointmentsToday,
            pendingInvoices,
            revenueMonth);

        return Result<ClinicStatsDto>.Success(stats);
    }
}
