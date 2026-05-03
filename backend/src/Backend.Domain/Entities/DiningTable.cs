using Backend.Domain.Common;

namespace Backend.Domain.Entities;

public sealed class DiningTable : AuditableEntity
{
    public string TableNumber { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public bool IsOccupied { get; set; }
}
