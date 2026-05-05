// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Doctors\DTOs\DoctorDtos.cs
using System;
using System.Collections.Generic;
using MediQueue.Domain.Enums;

namespace MediQueue.Application.Doctors.DTOs;

public class QualificationDto
{
    public string Degree { get; set; } = string.Empty;
    public string Institution { get; set; } = string.Empty;
    public int Year { get; set; }
}

public class DoctorDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public MedicalSpecialty Specialty { get; set; }
    public string? SubSpecialty { get; set; }
    public string LicenseNumber { get; set; } = string.Empty;
    public decimal ConsultationFee { get; set; }
    public decimal FollowUpFee { get; set; }
    public bool IsAvailable { get; set; }
}

public class DoctorDetailDto : DoctorDto
{
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Bio { get; set; }
    public int YearsOfExperience { get; set; }
    
    public List<QualificationDto> Qualifications { get; set; } = [];
    public List<WorkingShiftDto> WorkingShifts { get; set; } = [];
}

public class DoctorSummaryDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public MedicalSpecialty Specialty { get; set; }
    public decimal ConsultationFee { get; set; }
    public int YearsOfExperience { get; set; }
    public bool IsAvailable { get; set; }
}

public class WorkingShiftDto
{
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int SlotDurationMinutes { get; set; }
}

public class AvailableSlotDto
{
    public TimeOnly Time { get; set; }
    public bool IsBooked { get; set; }
}

public class DoctorAvailabilityDto
{
    public Guid DoctorId { get; set; }
    public DateTime Date { get; set; }
    public WorkingShiftDto? WorkingShift { get; set; }
    public List<AvailableSlotDto> Slots { get; set; } = [];
}
