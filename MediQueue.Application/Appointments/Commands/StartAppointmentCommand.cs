// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Appointments\Commands\StartAppointmentCommand.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using MediQueue.Application.Appointments.DTOs;
using MediQueue.Application.ClinicalVisits.Commands;
using MediQueue.Application.Common;
using MediQueue.Domain.Exceptions;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.Appointments.Commands;

public record StartAppointmentCommand(Guid AppointmentId) : ICommand<AppointmentDto>;

public class StartAppointmentCommandValidator : AbstractValidator<StartAppointmentCommand>
{
    public StartAppointmentCommandValidator()
    {
        RuleFor(x => x.AppointmentId).NotEmpty();
    }
}

public class StartAppointmentCommandHandler : IRequestHandler<StartAppointmentCommand, Result<AppointmentDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ISender _sender;

    public StartAppointmentCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ISender sender)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _sender = sender;
    }

    public async Task<Result<AppointmentDto>> Handle(StartAppointmentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var appointment = await _unitOfWork.Appointments.GetByIdAsync(request.AppointmentId);
            if (appointment == null)
            {
                return Result<AppointmentDto>.Failure($"Appointment with ID '{request.AppointmentId}' was not found.");
            }

            appointment.Start();

            await _unitOfWork.Appointments.UpdateAsync(appointment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var existingVisit = await _unitOfWork.ClinicalVisits.GetByAppointmentIdAsync(appointment.Id);
            if (existingVisit == null)
            {
                var createVisitResult = await _sender.Send(
                    new CreateClinicalVisitCommand { AppointmentId = appointment.Id },
                    cancellationToken);

                if (!createVisitResult.IsSuccess)
                {
                    return Result<AppointmentDto>.Failure(
                        $"Appointment started but clinical visit creation failed: {createVisitResult.Error}");
                }
            }

            return Result<AppointmentDto>.Success(_mapper.Map<AppointmentDto>(appointment));
        }
        catch (DomainException ex)
        {
            return Result<AppointmentDto>.Failure(ex.Message);
        }
    }
}
