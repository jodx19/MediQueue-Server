// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Patients\Queries\SearchPatientsQuery.cs
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Application.Interfaces;
using MediQueue.Application.Patients.DTOs;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.Patients.Queries;

public record SearchPatientsQuery(string SearchTerm, int PageNumber = 1, int PageSize = 20) : IQuery<PagedResult<PatientSummaryDto>>;

public class SearchPatientsQueryHandler : IRequestHandler<SearchPatientsQuery, Result<PagedResult<PatientSummaryDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICacheService _cacheService;

    public SearchPatientsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cacheService = cacheService;
    }

    public async Task<Result<PagedResult<PatientSummaryDto>>> Handle(SearchPatientsQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"patients:search:{request.SearchTerm}:{request.PageNumber}:{request.PageSize}";
        
        var cachedResult = await _cacheService.GetAsync<PagedResult<PatientSummaryDto>>(cacheKey);
        if (cachedResult != null)
        {
            return Result<PagedResult<PatientSummaryDto>>.Success(cachedResult);
        }

        var pagedPatients = await _unitOfWork.Patients.SearchAsync(request.SearchTerm, request.PageNumber, request.PageSize);
        
        // AutoMapper ProjectTo is usually used with IQueryable, but since repository returns PagedResult<Patient>, we map the Items.
        var itemsDto = pagedPatients.Items.Select(p => _mapper.Map<PatientSummaryDto>(p)).ToList();
        
        var result = PagedResult<PatientSummaryDto>.Create(itemsDto, pagedPatients.TotalCount, pagedPatients.PageNumber, pagedPatients.PageSize);

        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(2));

        return Result<PagedResult<PatientSummaryDto>>.Success(result);
    }
}
