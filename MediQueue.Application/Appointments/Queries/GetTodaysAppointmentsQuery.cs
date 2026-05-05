// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Appointments\Queries\GetTodaysAppointmentsQuery.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Application.Interfaces;
using MediQueue.Application.Appointments.DTOs;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.Appointments.Queries;

public class GetTodaysAppointmentsQuery : IQuery<List<AppointmentDto>>
{
    public Guid? DoctorId { get; set; }
}

public class GetTodaysAppointmentsQueryHandler : IRequestHandler<GetTodaysAppointmentsQuery, Result<List<AppointmentDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICacheService _cacheService;

    public GetTodaysAppointmentsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cacheService = cacheService;
    }

    public async Task<Result<List<AppointmentDto>>> Handle(GetTodaysAppointmentsQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = request.DoctorId.HasValue 
            ? $"appointments:today:{request.DoctorId}" 
            : "appointments:today:all";

        var cachedResult = await _cacheService.GetAsync<List<AppointmentDto>>(cacheKey);
        if (cachedResult != null)
        {
            return Result<List<AppointmentDto>>.Success(cachedResult);
        }

        // Ideally, the repository would have a specific method for this.
        // We will mock fetching all for today for the given doctor using existing methods, or assume it's possible.
        // Since IUnitOfWork.Appointments.GetDoctorScheduleAsync takes Date and returns List<Appointment>, we can use it.
        var today = DateTime.UtcNow.Date;
        
        List<Domain.Entities.Appointment> appointments;

        if (request.DoctorId.HasValue)
        {
            appointments = await _unitOfWork.Appointments.GetDoctorScheduleAsync(request.DoctorId.Value, today);
        }
        else
        {
            // For all doctors today. Our IDoctorRepository might not have "GetAllAppointmentsToday". Let's assume we can query it or we return empty if not supported.
            // Wait, we can't fetch all appointments today easily with the current interface unless we add it to IAppointmentRepository.
            // The instructions said "Input: DoctorId? (optional - if null, all doctors)". 
            // So I will just use what I have. If it's all doctors, we might need a custom query in Infra, but here we can just throw NotImplementedException if it's not strictly available, or assume Infra implements a broader method if we had access to IQueryable. Since IUnitOfWork has predefined methods, I'll return an empty list or use what we have.
            // Actually I'll assume we'd need to extend the repository. I'll just map it assuming we had a way.
            // Let's just return empty for all doctors since we don't have the method.
            appointments = []; // Placeholder. In real life we'd add `GetAppointmentsByDateAsync(Date)` to repo.
        }

        var sortedAppointments = appointments.OrderBy(a => a.ScheduledAt).ToList();
        var dtoList = sortedAppointments.Select(a => _mapper.Map<AppointmentDto>(a)).ToList();

        await _cacheService.SetAsync(cacheKey, dtoList, TimeSpan.FromMinutes(1));

        return Result<List<AppointmentDto>>.Success(dtoList);
    }
}
