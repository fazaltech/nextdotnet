namespace Backend.Application.Operations;

// IDs always refer to the active Rms schema. Amounts and prices are calculated on the server.
public sealed record OperationRequest
{
    public int Id { get; init; }
    public int TableId { get; init; }
    public int SessionId { get; init; }
    public int TargetId { get; init; }
    public int? CustomerId { get; init; }
    public int? Seat { get; init; }
    public int MenuItemId { get; init; }
    public int InventoryItemId { get; init; }
    public decimal Quantity { get; init; }
    public decimal UnitCost { get; init; }
    public decimal Amount { get; init; }
    public decimal Discount { get; init; }
    public decimal Tax { get; init; }
    public int Method { get; init; } = 1;
    public string? Name { get; init; }
    public string? Reference { get; init; }
    public string? Reason { get; init; }
    public string? Status { get; init; }
    public DateTime? Date { get; init; }
    public Guid RequestKey { get; init; }
    public int[] ItemIds { get; init; } = [];
    public OperationLine[] Items { get; init; } = [];
}
public sealed record OperationLine(int MenuItemId, int Quantity, int? CustomerId = null, int? Seat = null, string? Instructions = null);
public interface IOperationsRepository
{
    Task<object> ReadAsync(string section, int userId, bool allOrders, DateTime from, DateTime to, CancellationToken ct);
    Task<int> ExecuteAsync(string action, OperationRequest request, int userId, bool manager, CancellationToken ct);
    Task<IReadOnlyList<string>> PermissionsAsync(int userId, CancellationToken ct);
}
public interface IOperationsService
{
    Task<object> ReadAsync(string section, int userId, bool allOrders, DateTime from, DateTime to, CancellationToken ct);
    Task<int> ExecuteAsync(string action, OperationRequest request, int userId, bool manager, CancellationToken ct);
    Task<IReadOnlyList<string>> PermissionsAsync(int userId, CancellationToken ct);
}
