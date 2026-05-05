// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\ClinicalVisits\Queries\GetPatientPrescriptionsQuery.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Application.ClinicalVisits.DTOs;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.ClinicalVisits.Queries;

public record GetPatientPrescriptionsQuery(Guid PatientId, string Status = "All") : IQuery<List<PrescriptionDto>>;

public class GetPatientPrescriptionsQueryHandler : IRequestHandler<GetPatientPrescriptionsQuery, Result<List<PrescriptionDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetPatientPrescriptionsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<List<PrescriptionDto>>> Handle(GetPatientPrescriptionsQuery request, CancellationToken cancellationToken)
    {
        // This is a bit inefficient if a patient has many visits, as we load 100 recent visits to extract prescriptions.
        // A direct IQueryable or specialized repository method is better in production.
        var pagedVisits = await _unitOfWork.ClinicalVisits.GetPatientHistoryAsync(request.PatientId, 1, 100);
        
        var prescriptions = pagedVisits.Items
            .SelectMany(v => v.Prescriptions)
            .ToList();

        if (request.Status == "Active")
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            prescriptions = prescriptions.Where(p => p.ValidUntil >= today).ToList();
        }

        var sortedPrescriptions = prescriptions.OrderByDescending(p => p.IssuedAt).ToList();
        var dtoList = sortedPrescriptions.Select(p => _mapper.Map<PrescriptionDto>(p)).ToList();

        return Result<List<PrescriptionDto>>.Success(dtoList);
    }
}
