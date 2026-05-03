using Backend.Application.Abstractions;
using Backend.Domain.Enums;

namespace Backend.Application.Inventory;

public sealed record InventoryItemDto(int Id, string Name, string Unit, decimal Quantity, decimal ReorderLevel, decimal UnitCost, bool IsActive);
public sealed record InventoryItemUpsertRequest(string Name, string Unit, decimal Quantity, decimal ReorderLevel, decimal UnitCost, bool IsActive);
public sealed record StockTransactionRequest(int InventoryItemId, decimal Quantity, StockTransactionType TransactionType, string? Notes);

public interface IInventoryService : ICrudService<InventoryItemDto, InventoryItemUpsertRequest, InventoryItemUpsertRequest>
{
    Task<int> CreateStockTransactionAsync(StockTransactionRequest request, CancellationToken cancellationToken = default);
}
