// Path: MediQueue.Application/Appointments/Queries/GetAllAppointmentsQuery.cs
// Returns a paginated list of ALL appointments for the current tenant (staff view).
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Application.Appointments.DTOs;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.Appointments.Queries;

public record GetAllAppointmentsQuery(int Page = 1, int PageSize = 20)
    : IRequest<Result<PagedResult<AppointmentListItemDto>>>;

public sealed class GetAllAppointmentsQueryHandler
    : IRequestHandler<GetAllAppointmentsQuery, Result<PagedResult<AppointmentListItemDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllAppointmentsQueryHandler(IUnitOfWork unitOfWork)
        => _unitOfWork = unitOfWork;

    public async Task<Result<PagedResult<AppointmentListItemDto>>> Handle(
        GetAllAppointmentsQuery request, CancellationToken ct)
    {
        // Fetch a rolling window: past 1 year → future 6 months for the all-appointments view.
        var from = DateTime.UtcNow.AddYears(-1);
        var to   = DateTime.UtcNow.AddMonths(6);

        var all = await _unitOfWork.Appointments.GetByDateRangeAsync(from, to, null, ct);

        var visitMap = await _unitOfWork.ClinicalVisits.GetVisitIdsByAppointmentIdsAsync(
            all.Select(a => a.Id), ct);

        var totalCount = all.Count;

        var items = all
            .OrderByDescending(a => a.ScheduledAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => a.ToListItemDto(visitMap.TryGetValue(a.Id, out var vid) ? vid : null))
            .ToList();

        var paged = PagedResult<AppointmentListItemDto>.Create(
            items, totalCount, request.Page, request.PageSize);

        return Result<PagedResult<AppointmentListItemDto>>.Success(paged);
    }
}
