// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Appointments\Queries\GetUpcomingAppointmentsQuery.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Application.Appointments.DTOs;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.Appointments.Queries;

public record GetUpcomingAppointmentsQuery(int Days = 7, Guid? DoctorId = null) : IQuery<List<AppointmentDto>>;

public class GetUpcomingAppointmentsQueryHandler : IRequestHandler<GetUpcomingAppointmentsQuery, Result<List<AppointmentDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetUpcomingAppointmentsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<List<AppointmentDto>>> Handle(GetUpcomingAppointmentsQuery request, CancellationToken cancellationToken)
    {
        // This requires an infra method to fetch upcoming appointments by date range.
        // As a placeholder using our Domain.Interfaces, we will just use GetDoctorScheduleAsync for each day.
        var upcomingAppointments = new List<Domain.Entities.Appointment>();
        
        if (request.DoctorId.HasValue)
        {
            var today = DateTime.UtcNow.Date;
            for (int i = 0; i < request.Days; i++)
            {
                var dailyAppointments = await _unitOfWork.Appointments.GetDoctorScheduleAsync(request.DoctorId.Value, today.AddDays(i));
                upcomingAppointments.AddRange(dailyAppointments.Where(a => a.ScheduledAt > DateTime.UtcNow));
            }
        }
        else
        {
            // Similar to Today's query, we don't have a direct method for "all doctors" in the repository.
        }

        var sortedAppointments = upcomingAppointments.OrderBy(a => a.ScheduledAt).ToList();
        var dtoList = sortedAppointments.Select(a => _mapper.Map<AppointmentDto>(a)).ToList();

        return Result<List<AppointmentDto>>.Success(dtoList);
    }
}
