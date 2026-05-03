using Backend.Domain.Common;

namespace Backend.Domain.Entities;

public sealed class Customer : AuditableEntity
{
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
}
