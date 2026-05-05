// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\ClinicalVisits\Queries\GetPatientClinicalHistoryQuery.cs
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Application.ClinicalVisits.DTOs;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.ClinicalVisits.Queries;

public record GetPatientClinicalHistoryQuery(Guid PatientId, int PageNumber = 1, int PageSize = 20) : IQuery<PagedResult<ClinicalVisitSummaryDto>>;

public class GetPatientClinicalHistoryQueryHandler : IRequestHandler<GetPatientClinicalHistoryQuery, Result<PagedResult<ClinicalVisitSummaryDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetPatientClinicalHistoryQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<ClinicalVisitSummaryDto>>> Handle(GetPatientClinicalHistoryQuery request, CancellationToken cancellationToken)
    {
        var pagedVisits = await _unitOfWork.ClinicalVisits.GetPatientHistoryAsync(request.PatientId, request.PageNumber, request.PageSize);
        
        var itemsDto = pagedVisits.Items.Select(v => _mapper.Map<ClinicalVisitSummaryDto>(v)).ToList();
        var result = PagedResult<ClinicalVisitSummaryDto>.Create(itemsDto, pagedVisits.TotalCount, request.PageNumber, request.PageSize);

        return Result<PagedResult<ClinicalVisitSummaryDto>>.Success(result);
    }
}
