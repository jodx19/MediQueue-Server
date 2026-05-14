using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MediQueue.Application.Appointments.DTOs;
using MediQueue.Application.Common;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.Appointments.Queries;

public record GetAppointmentsInRangeQuery(
    DateTime From,
    DateTime To,
    Guid? DoctorId = null) : IQuery<List<AppointmentScheduleItemDto>>;

public class GetAppointmentsInRangeQueryHandler
    : IRequestHandler<GetAppointmentsInRangeQuery, Result<List<AppointmentScheduleItemDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAppointmentsInRangeQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<List<AppointmentScheduleItemDto>>> Handle(
        GetAppointmentsInRangeQuery request,
        CancellationToken cancellationToken)
    {
        var toExclusive = request.To.Kind == DateTimeKind.Utc
            ? request.To
            : request.To.ToUniversalTime();
        var fromUtc = request.From.Kind == DateTimeKind.Utc
            ? request.From
            : request.From.ToUniversalTime();

        var appointments = await _unitOfWork.Appointments.GetByDateRangeAsync(
            fromUtc,
            toExclusive,
            request.DoctorId,
            cancellationToken);

        var list = appointments
            .OrderBy(a => a.ScheduledAt)
            .Select(a => new AppointmentScheduleItemDto
            {
                AppointmentId = a.Id,
                PatientName = a.Patient?.PersonName.FullName ?? string.Empty,
                ScheduledAt = a.ScheduledAt,
                DurationMinutes = a.DurationMinutes,
                Status = a.Status.ToString(),
                ChiefComplaint = a.ChiefComplaint,
            })
            .ToList();

        return Result<List<AppointmentScheduleItemDto>>.Success(list);
    }
}
