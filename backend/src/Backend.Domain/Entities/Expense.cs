using Backend.Domain.Common;

namespace Backend.Domain.Entities;

public sealed class Expense : AuditableEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Category { get; set; }
    public decimal Amount { get; set; }
    public DateTime ExpenseDateUtc { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
}
