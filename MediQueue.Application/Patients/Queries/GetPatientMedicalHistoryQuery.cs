// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Patients\Queries\GetPatientMedicalHistoryQuery.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Application.Patients.DTOs;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.Patients.Queries;

public record GetPatientMedicalHistoryQuery(Guid PatientId) : IQuery<PatientMedicalHistoryDto>;

public class GetPatientMedicalHistoryQueryHandler : IRequestHandler<GetPatientMedicalHistoryQuery, Result<PatientMedicalHistoryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetPatientMedicalHistoryQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PatientMedicalHistoryDto>> Handle(GetPatientMedicalHistoryQuery request, CancellationToken cancellationToken)
    {
        var patient = await _unitOfWork.Patients.GetByIdAsync(request.PatientId);
        
        if (patient == null)
        {
            return Result<PatientMedicalHistoryDto>.Failure($"Patient with ID '{request.PatientId}' was not found.");
        }

        var dto = _mapper.Map<PatientMedicalHistoryDto>(patient);
        
        // Load the last 5 visits
        var visitsPage = await _unitOfWork.ClinicalVisits.GetPatientHistoryAsync(request.PatientId, 1, 5);
        
        // We will map this as basic objects or dynamic for now as requested. We'll populate with basic info.
        foreach (var visit in visitsPage.Items)
        {
            dto.LastVisitsSummary.Add(new 
            {
                visit.Id,
                visit.VisitDate,
                visit.AssessmentNote,
                visit.PlanNote
            });
        }

        return Result<PatientMedicalHistoryDto>.Success(dto);
    }
}
