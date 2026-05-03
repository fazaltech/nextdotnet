using Backend.Application.Inventory;
using Backend.Application.Persistence;

namespace Backend.Services.Inventory;

public sealed class InventoryService(IInventoryRepository repository) : IInventoryService
{
    public Task<IReadOnlyList<InventoryItemDto>> GetAllAsync(CancellationToken cancellationToken = default) => repository.GetAllAsync(cancellationToken);
    public Task<InventoryItemDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default) => repository.GetByIdAsync(id, cancellationToken);
    public Task<int> CreateAsync(InventoryItemUpsertRequest request, CancellationToken cancellationToken = default) => repository.CreateAsync(request, cancellationToken);
    public Task<bool> UpdateAsync(int id, InventoryItemUpsertRequest request, CancellationToken cancellationToken = default) => repository.UpdateAsync(id, request, cancellationToken);
    public Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default) => repository.DeleteAsync(id, cancellationToken);
    public Task<int> CreateStockTransactionAsync(StockTransactionRequest request, CancellationToken cancellationToken = default)
        => repository.CreateStockTransactionAsync(request, cancellationToken);
}
