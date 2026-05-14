using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Application.ClinicalVisits.DTOs;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.ClinicalVisits.Queries;

public record GetClinicalVisitByIdQuery(Guid VisitId) : IQuery<ClinicalVisitDetailDto>;

public class GetClinicalVisitByIdQueryHandler : IRequestHandler<GetClinicalVisitByIdQuery, Result<ClinicalVisitDetailDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetClinicalVisitByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ClinicalVisitDetailDto>> Handle(
        GetClinicalVisitByIdQuery request,
        CancellationToken cancellationToken)
    {
        var visit = await _unitOfWork.ClinicalVisits.GetByIdWithDetailsAsync(request.VisitId, cancellationToken);

        if (visit == null)
            return Result<ClinicalVisitDetailDto>.Failure($"ClinicalVisit with ID '{request.VisitId}' was not found.");

        var dto = _mapper.Map<ClinicalVisitDetailDto>(visit);
        await ClinicalVisitAttachmentMapper.PopulateAttachmentsAsync(_unitOfWork, dto, visit.Id, cancellationToken);

        return Result<ClinicalVisitDetailDto>.Success(dto);
    }
}
