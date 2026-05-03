using Backend.Application.Settings;

namespace Backend.Application.Persistence;

public interface ISettingsRepository
{
    Task<IReadOnlyList<AppSettingDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<AppSettingDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(AppSettingUpsertRequest request, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(int id, AppSettingUpsertRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
