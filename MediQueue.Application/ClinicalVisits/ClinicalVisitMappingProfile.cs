// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\ClinicalVisits\ClinicalVisitMappingProfile.cs
using AutoMapper;
using MediQueue.Domain.Entities;
using MediQueue.Application.ClinicalVisits.DTOs;

namespace MediQueue.Application.ClinicalVisits;

public class ClinicalVisitMappingProfile : Profile
{
    public ClinicalVisitMappingProfile()
    {
        CreateMap<ClinicalVisit, ClinicalVisitDto>();
        CreateMap<ClinicalVisit, ClinicalVisitDetailDto>()
            .IncludeBase<ClinicalVisit, ClinicalVisitDto>();
        CreateMap<ClinicalVisit, ClinicalVisitSummaryDto>()
            .IncludeBase<ClinicalVisit, ClinicalVisitDto>();

        CreateMap<Domain.ValueObjects.VitalSign, VitalSignDto>()
            .ForMember(d => d.Type, opt => opt.MapFrom(s => s.Type.ToString()));
            
        CreateMap<Diagnosis, DiagnosisDto>()
            .ForMember(d => d.Type, opt => opt.MapFrom(s => s.Type.ToString()))
            .ForMember(d => d.ICD10Code, opt => opt.MapFrom(s => s.MedicalCode.Code))
            .ForMember(d => d.CodeDescription, opt => opt.MapFrom(s => s.MedicalCode.Description));

        CreateMap<MedicalProcedure, MedicalProcedureDto>()
            .ForMember(d => d.CPTCode, opt => opt.MapFrom(s => s.MedicalCode.Code))
            .ForMember(d => d.Description, opt => opt.MapFrom(s => s.MedicalCode.Description))
            .ForMember(d => d.Fee, opt => opt.MapFrom(s => s.Fee.Amount));

        CreateMap<LabRequest, LabRequestDto>()
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()));

        CreateMap<ImagingRequest, ImagingRequestDto>()
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()));

        CreateMap<Referral, ReferralDto>()
            .ForMember(d => d.ReferredToSpecialty, opt => opt.MapFrom(s => s.ReferredToSpecialty.ToString()))
            .ForMember(d => d.Urgency, opt => opt.MapFrom(s => s.Urgency.ToString()));

        CreateMap<Prescription, PrescriptionDto>();
        CreateMap<MediQueue.Domain.ValueObjects.PrescriptionItem, PrescriptionItemDto>();
    }
}
