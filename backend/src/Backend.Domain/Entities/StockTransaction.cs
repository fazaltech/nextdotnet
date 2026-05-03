using Backend.Domain.Common;
using Backend.Domain.Enums;

namespace Backend.Domain.Entities;

public sealed class StockTransaction : AuditableEntity
{
    public int InventoryItemId { get; set; }
    public decimal Quantity { get; set; }
    public StockTransactionType TransactionType { get; set; }
    public string? Notes { get; set; }
}
