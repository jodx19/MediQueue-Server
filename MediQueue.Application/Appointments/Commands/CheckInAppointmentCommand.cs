// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Appointments\Commands\CheckInAppointmentCommand.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using MediQueue.Application.Appointments.DTOs;
using MediQueue.Application.Common;
using MediQueue.Domain.Exceptions;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.Appointments.Commands;

public record CheckInAppointmentCommand(Guid AppointmentId) : ICommand<AppointmentDto>;

public class CheckInAppointmentCommandValidator : AbstractValidator<CheckInAppointmentCommand>
{
    public CheckInAppointmentCommandValidator()
    {
        RuleFor(x => x.AppointmentId).NotEmpty();
    }
}

public class CheckInAppointmentCommandHandler : IRequestHandler<CheckInAppointmentCommand, Result<AppointmentDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CheckInAppointmentCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<AppointmentDto>> Handle(CheckInAppointmentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var appointment = await _unitOfWork.Appointments.GetByIdAsync(request.AppointmentId);
            if (appointment == null)
            {
                return Result<AppointmentDto>.Failure($"Appointment with ID '{request.AppointmentId}' was not found.");
            }

            appointment.CheckIn();

            await _unitOfWork.Appointments.UpdateAsync(appointment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<AppointmentDto>.Success(_mapper.Map<AppointmentDto>(appointment));
        }
        catch (DomainException ex)
        {
            return Result<AppointmentDto>.Failure(ex.Message);
        }
    }
}
