// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Patients\Queries\GetPatientByIdQuery.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Application.Patients.DTOs;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.Patients.Queries;

public record GetPatientByIdQuery(Guid PatientId) : IQuery<PatientDetailDto>;

public class GetPatientByIdQueryHandler : IRequestHandler<GetPatientByIdQuery, Result<PatientDetailDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetPatientByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PatientDetailDto>> Handle(GetPatientByIdQuery request, CancellationToken cancellationToken)
    {
        var patient = await _unitOfWork.Patients.GetByIdAsync(request.PatientId);
        
        if (patient == null)
        {
            return Result<PatientDetailDto>.Failure($"Patient with ID '{request.PatientId}' was not found.");
        }

        var dto = _mapper.Map<PatientDetailDto>(patient);
        return Result<PatientDetailDto>.Success(dto);
    }
}
