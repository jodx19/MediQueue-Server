// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Doctors\Queries\GetDoctorByIdQuery.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Application.Doctors.DTOs;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.Doctors.Queries;

public record GetDoctorByIdQuery(Guid DoctorId) : IQuery<DoctorDetailDto>;

public class GetDoctorByIdQueryHandler : IRequestHandler<GetDoctorByIdQuery, Result<DoctorDetailDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetDoctorByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<DoctorDetailDto>> Handle(GetDoctorByIdQuery request, CancellationToken cancellationToken)
    {
        var doctor = await _unitOfWork.Doctors.GetByIdAsync(request.DoctorId);
        
        if (doctor == null)
        {
            return Result<DoctorDetailDto>.Failure($"Doctor with ID '{request.DoctorId}' was not found.");
        }

        var dto = _mapper.Map<DoctorDetailDto>(doctor);
        return Result<DoctorDetailDto>.Success(dto);
    }
}
