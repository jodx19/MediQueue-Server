// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Appointments\Commands\MarkNoShowCommand.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MediQueue.Application.Appointments.DTOs;
using MediQueue.Application.Common;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.Appointments.Commands;

/// <summary>
/// Command to mark an appointment as a no-show.
/// </summary>
public record MarkNoShowCommand(Guid AppointmentId) : ICommand<AppointmentDto>;

/// <summary>
/// Handles the <see cref="MarkNoShowCommand"/>.
/// </summary>
public class MarkNoShowCommandHandler : IRequestHandler<MarkNoShowCommand, Result<AppointmentDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public MarkNoShowCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<AppointmentDto>> Handle(MarkNoShowCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _unitOfWork.Appointments.GetByIdAsync(request.AppointmentId);

        if (appointment is null)
            return Result<AppointmentDto>.Failure($"Appointment '{request.AppointmentId}' not found.");

        try
        {
            appointment.MarkNoShow();
            await _unitOfWork.Appointments.UpdateAsync(appointment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<AppointmentDto>.Success(_mapper.Map<AppointmentDto>(appointment));
        }
        catch (Domain.Exceptions.DomainException ex)
        {
            return Result<AppointmentDto>.Failure(ex.Message);
        }
    }
}
