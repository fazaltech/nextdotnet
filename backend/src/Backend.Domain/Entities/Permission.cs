using Backend.Domain.Common;

namespace Backend.Domain.Entities;

public sealed class Permission : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
