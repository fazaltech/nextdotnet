namespace Backend.Application.Settings;

public sealed record AppSettingDto(int Id, string SettingKey, string SettingValue, string? Description, bool IsActive);
public sealed record AppSettingUpsertRequest(string SettingKey, string SettingValue, string? Description, bool IsActive);

public interface ISettingsService
{
    Task<IReadOnlyList<AppSettingDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<AppSettingDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(AppSettingUpsertRequest request, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(int id, AppSettingUpsertRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
