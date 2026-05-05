// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Doctors\Queries\GetDoctorsBySpecialtyQuery.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Application.Interfaces;
using MediQueue.Application.Doctors.DTOs;
using MediQueue.Domain.Enums;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.Doctors.Queries;

public record GetDoctorsBySpecialtyQuery(MedicalSpecialty Specialty, int PageNumber = 1, int PageSize = 10) : IQuery<PagedResult<DoctorSummaryDto>>;

public class GetDoctorsBySpecialtyQueryHandler : IRequestHandler<GetDoctorsBySpecialtyQuery, Result<PagedResult<DoctorSummaryDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICacheService _cacheService;

    public GetDoctorsBySpecialtyQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cacheService = cacheService;
    }

    public async Task<Result<PagedResult<DoctorSummaryDto>>> Handle(GetDoctorsBySpecialtyQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"doctors:specialty:{request.Specialty}";
        
        var cachedResult = await _cacheService.GetAsync<List<DoctorSummaryDto>>(cacheKey);
        
        List<DoctorSummaryDto> allSpecialtyDoctors;

        if (cachedResult != null)
        {
            allSpecialtyDoctors = cachedResult;
        }
        else
        {
            var doctors = await _unitOfWork.Doctors.GetBySpecialtyAsync(request.Specialty);
            allSpecialtyDoctors = doctors.Select(d => _mapper.Map<DoctorSummaryDto>(d)).ToList();
            await _cacheService.SetAsync(cacheKey, allSpecialtyDoctors, TimeSpan.FromMinutes(10));
        }

        // Apply pagination
        var count = allSpecialtyDoctors.Count;
        var items = allSpecialtyDoctors.Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize).ToList();
        var result = PagedResult<DoctorSummaryDto>.Create(items, count, request.PageNumber, request.PageSize);

        return Result<PagedResult<DoctorSummaryDto>>.Success(result);
    }
}
