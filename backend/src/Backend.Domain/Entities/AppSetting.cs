using Backend.Domain.Common;

namespace Backend.Domain.Entities;

public sealed class AppSetting : AuditableEntity
{
    public string SettingKey { get; set; } = string.Empty;
    public string SettingValue { get; set; } = string.Empty;
    public string? Description { get; set; }
}
