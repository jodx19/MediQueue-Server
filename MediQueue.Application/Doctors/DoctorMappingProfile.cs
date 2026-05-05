// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Doctors\DoctorMappingProfile.cs
using AutoMapper;
using MediQueue.Domain.Entities;
using MediQueue.Domain.ValueObjects;
using MediQueue.Application.Doctors.DTOs;

namespace MediQueue.Application.Doctors;

public class DoctorMappingProfile : Profile
{
    public DoctorMappingProfile()
    {
        CreateMap<Doctor, DoctorDto>()
            .ForMember(d => d.FullName, opt => opt.MapFrom(s => s.PersonName.FullName))
            .ForMember(d => d.ConsultationFee, opt => opt.MapFrom(s => s.ConsultationFee.Amount))
            .ForMember(d => d.FollowUpFee, opt => opt.MapFrom(s => s.FollowUpFee.Amount));

        CreateMap<Doctor, DoctorDetailDto>()
            .IncludeBase<Doctor, DoctorDto>()
            .ForMember(d => d.Phone, opt => opt.MapFrom(s => s.ContactInfo.Phone))
            .ForMember(d => d.Email, opt => opt.MapFrom(s => s.ContactInfo.Email));

        CreateMap<Doctor, DoctorSummaryDto>()
            .ForMember(d => d.FullName, opt => opt.MapFrom(s => s.PersonName.FullName))
            .ForMember(d => d.ConsultationFee, opt => opt.MapFrom(s => s.ConsultationFee.Amount));

        CreateMap<DoctorQualification, QualificationDto>();
        CreateMap<WorkingShift, WorkingShiftDto>();
    }
}
