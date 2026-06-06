using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Application.Interfaces;
using MediQueue.Application.Settings.Dtos;

namespace MediQueue.Application.Settings.Commands;

public record UpdateSettingsCommand(
    string ClinicName,
    string ClinicPhone,
    string ClinicEmail,
    string ClinicAddress,
    string WorkStartTime,
    string WorkEndTime,
    int AppointmentDurationMinutes,
    string Currency,
    string TimeZone,
    bool AllowOnlineBooking,
    bool RequireDepositForBooking,
    decimal DepositAmount
) : ICommand<ClinicSettingsDto>;

public class UpdateSettingsCommandValidator : AbstractValidator<UpdateSettingsCommand>
{
    public UpdateSettingsCommandValidator()
    {
        RuleFor(x => x.ClinicName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.WorkStartTime).Matches(@"^([01]?[0-9]|2[0-3]):[0-5][0-9]$").WithMessage("Invalid WorkStartTime format. Use HH:mm.");
        RuleFor(x => x.WorkEndTime).Matches(@"^([01]?[0-9]|2[0-3]):[0-5][0-9]$").WithMessage("Invalid WorkEndTime format. Use HH:mm.");
        RuleFor(x => x.AppointmentDurationMinutes).InclusiveBetween(10, 120);
        RuleFor(x => x.Currency).Length(3);
        RuleFor(x => x.DepositAmount).GreaterThanOrEqualTo(0);
    }
}

public class UpdateSettingsCommandHandler : IRequestHandler<UpdateSettingsCommand, Result<ClinicSettingsDto>>
{
    private readonly ISettingsRepository _settingsRepository;

    public UpdateSettingsCommandHandler(ISettingsRepository settingsRepository)
    {
        _settingsRepository = settingsRepository;
    }

    public async Task<Result<ClinicSettingsDto>> Handle(UpdateSettingsCommand request, CancellationToken cancellationToken)
    {
        var dto = new ClinicSettingsDto(
            System.Guid.Empty,
            request.ClinicName,
            request.ClinicPhone,
            request.ClinicEmail,
            request.ClinicAddress,
            string.Empty,
            request.WorkStartTime,
            request.WorkEndTime,
            request.AppointmentDurationMinutes,
            request.Currency,
            request.TimeZone,
            request.AllowOnlineBooking,
            request.RequireDepositForBooking,
            request.DepositAmount
        );

        var updatedSettings = await _settingsRepository.UpdateSettingsAsync(dto, cancellationToken);
        
        return Result<ClinicSettingsDto>.Success(updatedSettings);
    }
}
