// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Appointments\Queries\GetAppointmentByIdQuery.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Application.Appointments.DTOs;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.Appointments.Queries;

public record GetAppointmentByIdQuery(Guid AppointmentId) : IQuery<AppointmentDetailDto>;

public class GetAppointmentByIdQueryHandler : IRequestHandler<GetAppointmentByIdQuery, Result<AppointmentDetailDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAppointmentByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<AppointmentDetailDto>> Handle(GetAppointmentByIdQuery request, CancellationToken cancellationToken)
    {
        var appointment = await _unitOfWork.Appointments.GetByIdAsync(request.AppointmentId);
        if (appointment == null)
        {
            return Result<AppointmentDetailDto>.Failure($"Appointment with ID '{request.AppointmentId}' was not found.");
        }

        var dto = _mapper.Map<AppointmentDetailDto>(appointment);
        return Result<AppointmentDetailDto>.Success(dto);
    }
}
