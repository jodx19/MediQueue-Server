using System.Threading;
using System.Threading.Tasks;
using MediQueue.Application.Settings.Dtos;

namespace MediQueue.Application.Interfaces;

public interface ISettingsRepository
{
    Task<ClinicSettingsDto> GetSettingsAsync(CancellationToken cancellationToken);
    Task<ClinicSettingsDto> UpdateSettingsAsync(ClinicSettingsDto dto, CancellationToken cancellationToken);
}
