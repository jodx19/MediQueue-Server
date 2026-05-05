// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\ClinicalVisits\Queries\GetVisitByAppointmentQuery.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Application.ClinicalVisits.DTOs;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.ClinicalVisits.Queries;

public record GetVisitByAppointmentQuery(Guid AppointmentId) : IQuery<ClinicalVisitDetailDto>;

public class GetVisitByAppointmentQueryHandler : IRequestHandler<GetVisitByAppointmentQuery, Result<ClinicalVisitDetailDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetVisitByAppointmentQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ClinicalVisitDetailDto>> Handle(GetVisitByAppointmentQuery request, CancellationToken cancellationToken)
    {
        var visit = await _unitOfWork.ClinicalVisits.GetByAppointmentIdAsync(request.AppointmentId);
        
        if (visit == null)
        {
            return Result<ClinicalVisitDetailDto>.Failure($"ClinicalVisit for Appointment '{request.AppointmentId}' was not found.");
        }

        var dto = _mapper.Map<ClinicalVisitDetailDto>(visit);
        return Result<ClinicalVisitDetailDto>.Success(dto);
    }
}
