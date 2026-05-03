using Backend.Domain.Common;

namespace Backend.Domain.Entities;

public sealed class MenuItem : AuditableEntity
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; } = true;
}
