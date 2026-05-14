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

public record GetDoctorScheduleQuery(Guid DoctorId, DateTime Date) : IQuery<List<AppointmentScheduleItemDto>>;

public class GetDoctorScheduleQueryHandler : IRequestHandler<GetDoctorScheduleQuery, Result<List<AppointmentScheduleItemDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetDoctorScheduleQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<List<AppointmentScheduleItemDto>>> Handle(GetDoctorScheduleQuery request, CancellationToken cancellationToken)
    {
        var appointments = await _unitOfWork.Appointments.GetDoctorScheduleAsync(request.DoctorId, request.Date.Date);

        var dtoList = appointments
            .OrderBy(a => a.ScheduledAt)
            .Select(a => _mapper.Map<AppointmentScheduleItemDto>(a))
            .ToList();

        return Result<List<AppointmentScheduleItemDto>>.Success(dtoList);
    }
}
