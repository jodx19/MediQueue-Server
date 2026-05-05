// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Patients\PatientMappingProfile.cs
using AutoMapper;
using MediQueue.Domain.Entities;
using MediQueue.Application.Patients.DTOs;

namespace MediQueue.Application.Patients;

public class PatientMappingProfile : Profile
{
    public PatientMappingProfile()
    {
        CreateMap<Patient, PatientDto>()
            .ForMember(d => d.FullName, opt => opt.MapFrom(s => s.PersonName.FullName))
            .ForMember(d => d.Phone, opt => opt.MapFrom(s => s.ContactInfo.Phone))
            .ForMember(d => d.Email, opt => opt.MapFrom(s => s.ContactInfo.Email));

        CreateMap<Patient, PatientDetailDto>()
            .IncludeBase<Patient, PatientDto>()
            .ForMember(d => d.Street, opt => opt.MapFrom(s => s.Address.Street))
            .ForMember(d => d.City, opt => opt.MapFrom(s => s.Address.City))
            .ForMember(d => d.Governorate, opt => opt.MapFrom(s => s.Address.Governorate))
            .ForMember(d => d.Country, opt => opt.MapFrom(s => s.Address.Country));

        CreateMap<Patient, PatientSummaryDto>()
            .ForMember(d => d.FullName, opt => opt.MapFrom(s => s.PersonName.FullName))
            .ForMember(d => d.Phone, opt => opt.MapFrom(s => s.ContactInfo.Phone));

        CreateMap<Allergy, AllergyDto>()
            .ForMember(d => d.Severity, opt => opt.MapFrom(s => s.Severity.ToString()));
            
        CreateMap<ChronicCondition, ChronicConditionDto>();
        CreateMap<CurrentMedication, CurrentMedicationDto>();
        
        CreateMap<Patient, PatientMedicalHistoryDto>()
            .ForMember(d => d.PatientId, opt => opt.MapFrom(s => s.Id))
            .ForMember(d => d.FullName, opt => opt.MapFrom(s => s.PersonName.FullName))
            .ForMember(d => d.LastVisitsSummary, opt => opt.Ignore()); // Will be filled manually or by another map
    }
}
