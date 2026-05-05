// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Doctors\Queries\GetDoctorAvailabilityQuery.cs
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Application.Interfaces;
using MediQueue.Application.Doctors.DTOs;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.Doctors.Queries;

public record GetDoctorAvailabilityQuery(Guid DoctorId, DateTime Date) : IQuery<DoctorAvailabilityDto>;

public class GetDoctorAvailabilityQueryHandler : IRequestHandler<GetDoctorAvailabilityQuery, Result<DoctorAvailabilityDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICacheService _cacheService;

    public GetDoctorAvailabilityQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cacheService = cacheService;
    }

    public async Task<Result<DoctorAvailabilityDto>> Handle(GetDoctorAvailabilityQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"availability:{request.DoctorId}:{request.Date:yyyy-MM-dd}";
        
        var cachedResult = await _cacheService.GetAsync<DoctorAvailabilityDto>(cacheKey);
        if (cachedResult != null)
        {
            return Result<DoctorAvailabilityDto>.Success(cachedResult);
        }

        var doctor = await _unitOfWork.Doctors.GetByIdAsync(request.DoctorId);
        if (doctor == null)
        {
            return Result<DoctorAvailabilityDto>.Failure($"Doctor with ID '{request.DoctorId}' was not found.");
        }

        var targetDate = request.Date.Date;
        var dayOfWeek = targetDate.DayOfWeek;

        var shift = doctor.WorkingShifts.FirstOrDefault(s => s.DayOfWeek == dayOfWeek);
        if (shift == null)
        {
            // Doctor is not working on this day
            var emptyResult = new DoctorAvailabilityDto
            {
                DoctorId = request.DoctorId,
                Date = targetDate,
                WorkingShift = null,
                Slots = []
            };
            return Result<DoctorAvailabilityDto>.Success(emptyResult);
        }

        var shiftDto = _mapper.Map<WorkingShiftDto>(shift);
        
        var appointments = await _unitOfWork.Appointments.GetDoctorScheduleAsync(request.DoctorId, targetDate);
        
        // Count bookings per slot start time
        var slotBookings = appointments
            .Where(a => a.Status == Domain.Enums.AppointmentStatus.Scheduled || a.Status == Domain.Enums.AppointmentStatus.Confirmed)
            .GroupBy(a => TimeOnly.FromDateTime(a.ScheduledAt))
            .ToDictionary(g => g.Key, g => g.Count());

        var availableSlots = shift.GenerateSlots()
            .Select(slotTime => new AvailableSlotDto
            {
                Time = slotTime,
                IsBooked = slotBookings.ContainsKey(slotTime) && slotBookings[slotTime] >= shift.MaxPatientsPerSlot
            })
            .ToList();

        var resultDto = new DoctorAvailabilityDto
        {
            DoctorId = request.DoctorId,
            Date = targetDate,
            WorkingShift = shiftDto,
            Slots = availableSlots
        };

        await _cacheService.SetAsync(cacheKey, resultDto, TimeSpan.FromMinutes(5));

        return Result<DoctorAvailabilityDto>.Success(resultDto);
    }
}
