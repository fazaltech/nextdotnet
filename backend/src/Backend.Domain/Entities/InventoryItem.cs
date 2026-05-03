using Backend.Domain.Common;

namespace Backend.Domain.Entities;

public sealed class InventoryItem : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = "pcs";
    public decimal Quantity { get; set; }
    public decimal ReorderLevel { get; set; }
    public decimal UnitCost { get; set; }
}
