using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Application.Interfaces;
using MediQueue.Application.Appointments;
using MediQueue.Application.Appointments.DTOs;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.Appointments.Queries;

public record GetTodaysAppointmentsQuery(Guid? DoctorId = null) : IQuery<List<AppointmentListItemDto>>;

public class GetTodaysAppointmentsQueryHandler
    : IRequestHandler<GetTodaysAppointmentsQuery, Result<List<AppointmentListItemDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;

    public GetTodaysAppointmentsQueryHandler(IUnitOfWork unitOfWork, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task<Result<List<AppointmentListItemDto>>> Handle(
        GetTodaysAppointmentsQuery request,
        CancellationToken cancellationToken)
    {
        var cacheKey = request.DoctorId.HasValue
            ? $"appointments:today:list:{request.DoctorId}"
            : "appointments:today:list:all";

        var cached = await _cacheService.GetAsync<List<AppointmentListItemDto>>(cacheKey);
        if (cached != null)
            return Result<List<AppointmentListItemDto>>.Success(cached);

        var start = DateTime.UtcNow.Date;
        var end = start.AddDays(1);

        var appointments = await _unitOfWork.Appointments.GetByDateRangeAsync(
            start,
            end,
            request.DoctorId,
            cancellationToken);

        var visitMap = await _unitOfWork.ClinicalVisits.GetVisitIdsByAppointmentIdsAsync(
            appointments.Select(a => a.Id),
            cancellationToken);

        var dtoList = appointments
            .OrderBy(a => a.ScheduledAt)
            .Select(a => a.ToListItemDto(visitMap.TryGetValue(a.Id, out var vid) ? vid : null))
            .ToList();

        await _cacheService.SetAsync(cacheKey, dtoList, TimeSpan.FromMinutes(1));

        return Result<List<AppointmentListItemDto>>.Success(dtoList);
    }
}
