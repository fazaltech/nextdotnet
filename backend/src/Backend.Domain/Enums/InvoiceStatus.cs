namespace Backend.Domain.Enums;

public enum InvoiceStatus
{
    Unpaid = 1,
    PartiallyPaid = 2,
    Paid = 3,
    Cancelled = 4,
    Refunded = 5
}
