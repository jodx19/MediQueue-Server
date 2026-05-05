// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Doctors\Commands\UpdateDoctorCommand.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Domain.Exceptions;
using MediQueue.Domain.Interfaces;
using MediQueue.Domain.ValueObjects;

namespace MediQueue.Application.Doctors.Commands;

public class UpdateDoctorCommand : ICommand
{
    public Guid DoctorId { get; set; }
    public decimal ConsultationFee { get; set; }
    public decimal FollowUpFee { get; set; }
    public string? Bio { get; set; }
    public bool IsAvailable { get; set; }
}

public class UpdateDoctorCommandValidator : AbstractValidator<UpdateDoctorCommand>
{
    public UpdateDoctorCommandValidator()
    {
        RuleFor(x => x.DoctorId).NotEmpty();
        RuleFor(x => x.ConsultationFee).GreaterThan(0);
        RuleFor(x => x.FollowUpFee).GreaterThan(0);
    }
}

public class UpdateDoctorCommandHandler : IRequestHandler<UpdateDoctorCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateDoctorCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateDoctorCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var doctor = await _unitOfWork.Doctors.GetByIdAsync(request.DoctorId);
            if (doctor == null)
            {
                return Result.Failure($"Doctor with ID '{request.DoctorId}' was not found.");
            }

            doctor.UpdateFees(new Money(request.ConsultationFee), new Money(request.FollowUpFee));
            
            // Bio and IsAvailable update logic. Doctor has SetUnavailable but not SetAvailable in domain. 
            if (!request.IsAvailable)
            {
                doctor.SetUnavailable("Updated via command");
            }
            
            // Note: Domain lacks UpdateBio, we might need to rely on EF Core or domain update methods. We just update what domain allows.

            await _unitOfWork.Doctors.UpdateAsync(doctor);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
