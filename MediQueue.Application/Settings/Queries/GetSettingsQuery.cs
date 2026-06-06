using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Application.Interfaces;
using MediQueue.Application.Settings.Dtos;

namespace MediQueue.Application.Settings.Queries;

public record GetSettingsQuery : IQuery<ClinicSettingsDto>;

public class GetSettingsQueryHandler : IRequestHandler<GetSettingsQuery, Result<ClinicSettingsDto>>
{
    private readonly ISettingsRepository _settingsRepository;

    public GetSettingsQueryHandler(ISettingsRepository settingsRepository)
    {
        _settingsRepository = settingsRepository;
    }

    public async Task<Result<ClinicSettingsDto>> Handle(GetSettingsQuery request, CancellationToken cancellationToken)
    {
        var settings = await _settingsRepository.GetSettingsAsync(cancellationToken);
        return Result<ClinicSettingsDto>.Success(settings);
    }
}
