using Backend.Domain.Common;
using Backend.Domain.Enums;

namespace Backend.Domain.Entities;

public sealed class SalesOrder : AuditableEntity
{
    public int TableId { get; set; }
    public int? CustomerId { get; set; }
    public DateTime OrderDateUtc { get; set; } = DateTime.UtcNow;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
}
