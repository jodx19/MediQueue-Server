// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Appointments\AppointmentMappingProfile.cs
using AutoMapper;
using MediQueue.Domain.Entities;
using MediQueue.Application.Appointments.DTOs;

namespace MediQueue.Application.Appointments;

public class AppointmentMappingProfile : Profile
{
    public AppointmentMappingProfile()
    {
        CreateMap<Appointment, AppointmentDto>()
            .ForMember(d => d.PatientName, opt => opt.MapFrom(s => s.Patient.PersonName.FullName))
            .ForMember(d => d.DoctorName, opt => opt.MapFrom(s => s.Doctor.PersonName.FullName));

        CreateMap<Appointment, AppointmentDetailDto>()
            .IncludeBase<Appointment, AppointmentDto>();

        CreateMap<Appointment, AppointmentScheduleItemDto>()
            .ForMember(d => d.AppointmentId, opt => opt.MapFrom(s => s.Id))
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.PatientName, opt => opt.MapFrom(s => s.Patient != null ? s.Patient.PersonName.FullName : string.Empty));
    }
}
