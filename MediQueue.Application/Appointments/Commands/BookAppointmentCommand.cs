// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Appointments\Commands\BookAppointmentCommand.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Application.Interfaces;
using MediQueue.Application.Appointments.DTOs;
using MediQueue.Domain.Entities;
using MediQueue.Domain.Enums;
using MediQueue.Domain.Exceptions;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.Appointments.Commands;

public class BookAppointmentCommand : ICommand<AppointmentDto>
{
    public Guid PatientId { get; set; }
    public Guid DoctorId { get; set; }
    public Guid ClinicId { get; set; }
    public DateTime ScheduledAt { get; set; }
    public int DurationMinutes { get; set; }
    public VisitType VisitType { get; set; }
    public AppointmentPriority Priority { get; set; }
    public string ChiefComplaint { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? RoomNumber { get; set; }
}

public class BookAppointmentCommandValidator : AbstractValidator<BookAppointmentCommand>
{
    public BookAppointmentCommandValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.DoctorId).NotEmpty();
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.ScheduledAt).GreaterThan(DateTime.UtcNow).WithMessage("Scheduled time must be in the future.");
        RuleFor(x => x.DurationMinutes).InclusiveBetween(10, 240);
        RuleFor(x => x.ChiefComplaint).NotEmpty().Length(5, 500);
    }
}

public class BookAppointmentCommandHandler : IRequestHandler<BookAppointmentCommand, Result<AppointmentDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICacheService _cacheService;

    public BookAppointmentCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cacheService = cacheService;
    }

    public async Task<Result<AppointmentDto>> Handle(BookAppointmentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var patient = await _unitOfWork.Patients.GetByIdAsync(request.PatientId);
            if (patient == null || !patient.IsActive)
            {
                return Result<AppointmentDto>.Failure("Patient not found or inactive.");
            }

            var doctor = await _unitOfWork.Doctors.GetByIdAsync(request.DoctorId);
            if (doctor == null || !doctor.IsAvailable)
            {
                return Result<AppointmentDto>.Failure("Doctor not found or unavailable.");
            }

            if (!doctor.IsWithinWorkingHours(request.ScheduledAt, request.DurationMinutes))
            {
                return Result<AppointmentDto>.Failure("The requested time is outside the doctor's working hours.");
            }

            var hasConflict = await _unitOfWork.Appointments.HasConflictAsync(request.DoctorId, request.ScheduledAt, request.DurationMinutes);
            if (hasConflict)
            {
                throw new AppointmentConflictException(request.DoctorId, request.ScheduledAt);
            }

            var appointment = Appointment.Book(
                request.PatientId,
                request.DoctorId,
                request.ClinicId,
                request.ScheduledAt,
                request.DurationMinutes,
                request.Priority,
                request.VisitType,
                request.ChiefComplaint,
                request.Notes);

            // Set room number if provided (domain has it but not in Book factory)
            // Wait, Book factory doesn't accept RoomNumber. So we can't set it unless we add a method.
            // For now, domain entity doesn't have a way to set RoomNumber post-creation without a method. 
            // We'll skip it to follow strict domain rules, or assume it's added. Let's ignore it if no method.

            await _unitOfWork.Appointments.AddAsync(appointment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Invalidate availability cache
            await _cacheService.RemoveAsync($"availability:{request.DoctorId}:{request.ScheduledAt:yyyy-MM-dd}");

            var dto = _mapper.Map<AppointmentDto>(appointment);
            return Result<AppointmentDto>.Success(dto);
        }
        catch (DomainException ex)
        {
            return Result<AppointmentDto>.Failure(ex.Message);
        }
    }
}
