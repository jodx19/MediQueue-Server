// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Patients\Queries\GetPatientByMRNQuery.cs
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Application.Patients.DTOs;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.Patients.Queries;

public record GetPatientByMRNQuery(string MRN) : IQuery<PatientDetailDto>;

public class GetPatientByMRNQueryHandler : IRequestHandler<GetPatientByMRNQuery, Result<PatientDetailDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetPatientByMRNQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PatientDetailDto>> Handle(GetPatientByMRNQuery request, CancellationToken cancellationToken)
    {
        var patient = await _unitOfWork.Patients.GetByMRNAsync(request.MRN);
        
        if (patient == null)
        {
            return Result<PatientDetailDto>.Failure($"Patient with MRN '{request.MRN}' was not found.");
        }

        var dto = _mapper.Map<PatientDetailDto>(patient);
        return Result<PatientDetailDto>.Success(dto);
    }
}
