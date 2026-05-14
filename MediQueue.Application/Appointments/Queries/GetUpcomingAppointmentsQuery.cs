using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Application.Appointments;
using MediQueue.Application.Appointments.DTOs;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.Appointments.Queries;

public record GetUpcomingAppointmentsQuery(int Days = 7, Guid? DoctorId = null)
    : IQuery<List<AppointmentListItemDto>>;

public class GetUpcomingAppointmentsQueryHandler
    : IRequestHandler<GetUpcomingAppointmentsQuery, Result<List<AppointmentListItemDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetUpcomingAppointmentsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<List<AppointmentListItemDto>>> Handle(
        GetUpcomingAppointmentsQuery request,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var end = now.AddDays(Math.Max(1, request.Days));

        var appointments = await _unitOfWork.Appointments.GetByDateRangeAsync(
            now,
            end,
            request.DoctorId,
            cancellationToken);

        var future = appointments
            .Where(a => a.ScheduledAt > now)
            .OrderBy(a => a.ScheduledAt)
            .ToList();

        var visitMap = await _unitOfWork.ClinicalVisits.GetVisitIdsByAppointmentIdsAsync(
            future.Select(a => a.Id),
            cancellationToken);

        var dtoList = future
            .Select(a => a.ToListItemDto(visitMap.TryGetValue(a.Id, out var vid) ? vid : null))
            .ToList();

        return Result<List<AppointmentListItemDto>>.Success(dtoList);
    }
}
