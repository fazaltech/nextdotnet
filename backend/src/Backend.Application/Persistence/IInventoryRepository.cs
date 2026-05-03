using Backend.Application.Inventory;

namespace Backend.Application.Persistence;

public interface IInventoryRepository
{
    Task<IReadOnlyList<InventoryItemDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<InventoryItemDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(InventoryItemUpsertRequest request, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(int id, InventoryItemUpsertRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CreateStockTransactionAsync(StockTransactionRequest request, CancellationToken cancellationToken = default);
}
