// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Appointments\Queries\GetPatientAppointmentsQuery.cs
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Application.Appointments.DTOs;
using MediQueue.Domain.Enums;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.Appointments.Queries;

public record GetPatientAppointmentsQuery(Guid PatientId, int PageNumber = 1, int PageSize = 20, AppointmentStatus? Status = null) : IQuery<PagedResult<AppointmentDto>>;

public class GetPatientAppointmentsQueryHandler : IRequestHandler<GetPatientAppointmentsQuery, Result<PagedResult<AppointmentDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetPatientAppointmentsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<AppointmentDto>>> Handle(GetPatientAppointmentsQuery request, CancellationToken cancellationToken)
    {
        var pagedAppointments = await _unitOfWork.Appointments.GetPatientHistoryAsync(request.PatientId, request.PageNumber, request.PageSize);
        
        var items = pagedAppointments.Items;
        if (request.Status.HasValue)
        {
            items = items.Where(a => a.Status == request.Status.Value).ToList();
        }

        var itemsDto = items.Select(a => _mapper.Map<AppointmentDto>(a)).ToList();
        var result = PagedResult<AppointmentDto>.Create(itemsDto, pagedAppointments.TotalCount, request.PageNumber, request.PageSize);

        return Result<PagedResult<AppointmentDto>>.Success(result);
    }
}
