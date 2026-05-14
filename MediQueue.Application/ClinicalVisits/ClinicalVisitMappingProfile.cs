// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\ClinicalVisits\ClinicalVisitMappingProfile.cs
using System.Linq;
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
            .IncludeBase<ClinicalVisit, ClinicalVisitDto>()
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.IsFinalized ? "Finalized" : "InProgress"))
            .ForMember(d => d.StartedAt, opt => opt.MapFrom(s => s.VisitDate))
            .ForMember(d => d.EndedAt, opt => opt.MapFrom(s => s.Appointment != null ? s.Appointment.ActualEndTime : null))
            .ForMember(d => d.PatientName, opt => opt.MapFrom(s => s.Patient.PersonName.FullName))
            .ForMember(d => d.PatientMrn, opt => opt.MapFrom(s => s.Patient.MedicalRecordNumber))
            .ForMember(d => d.BloodType, opt => opt.MapFrom(s => s.Patient.BloodType.ToString()))
            .ForMember(d => d.Allergies, opt => opt.MapFrom(s => s.Patient.Allergies.Select(a => a.Allergen).ToList()))
            .ForMember(d => d.ChronicConditions, opt => opt.MapFrom(s => s.Patient.ChronicConditions.Select(c => c.ConditionName).ToList()))
            .ForMember(d => d.Subjective, opt => opt.MapFrom(s => s.SubjectiveNote))
            .ForMember(d => d.Objective, opt => opt.MapFrom(s => s.ObjectiveNote))
            .ForMember(d => d.Assessment, opt => opt.MapFrom(s => s.AssessmentNote))
            .ForMember(d => d.Plan, opt => opt.MapFrom(s => s.PlanNote))
            .ForMember(d => d.Prescriptions, opt => opt.MapFrom(s => s.Prescriptions.ToList()))
            .ForMember(d => d.Attachments, opt => opt.Ignore());
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
