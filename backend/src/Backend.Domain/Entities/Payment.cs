using Backend.Domain.Common;
using Backend.Domain.Enums;

namespace Backend.Domain.Entities;

public sealed class Payment : AuditableEntity
{
    public int InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; } = PaymentMethod.Cash;
    public PaymentStatus Status { get; set; } = PaymentStatus.Completed;
    public DateTime PaymentDateUtc { get; set; } = DateTime.UtcNow;
    public string? TransactionReference { get; set; }
}
