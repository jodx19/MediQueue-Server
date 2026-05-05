// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Doctors\Queries\GetAllDoctorsQuery.cs
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Application.Doctors.DTOs;
using MediQueue.Domain.Enums;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.Doctors.Queries;

public record GetAllDoctorsQuery(int PageNumber = 1, int PageSize = 10, MedicalSpecialty? Specialty = null) : IQuery<PagedResult<DoctorSummaryDto>>;

public class GetAllDoctorsQueryHandler : IRequestHandler<GetAllDoctorsQuery, Result<PagedResult<DoctorSummaryDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllDoctorsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<DoctorSummaryDto>>> Handle(GetAllDoctorsQuery request, CancellationToken cancellationToken)
    {
        var pagedDoctors = await _unitOfWork.Doctors.GetAllAsync(request.PageNumber, request.PageSize);
        
        var items = pagedDoctors.Items;
        if (request.Specialty.HasValue)
        {
            items = items.Where(d => d.Specialty == request.Specialty.Value).ToList();
            // Note: In a real world scenario, the filter should be passed to the repository 
            // instead of fetching all and filtering in memory. We're adapting to the given IDoctorRepository.
        }

        var itemsDto = items.Select(d => _mapper.Map<DoctorSummaryDto>(d)).ToList();
        var result = PagedResult<DoctorSummaryDto>.Create(itemsDto, pagedDoctors.TotalCount, request.PageNumber, request.PageSize);

        return Result<PagedResult<DoctorSummaryDto>>.Success(result);
    }
}
