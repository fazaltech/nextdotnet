using Backend.Application.MenuItems;
using Backend.Application.Persistence;

namespace Backend.Services.MenuItems;

public sealed class MenuItemService(IMenuItemRepository repository) : IMenuItemService
{
    public Task<IReadOnlyList<MenuItemDto>> GetAllAsync(CancellationToken cancellationToken = default) => repository.GetAllAsync(cancellationToken);
    public Task<MenuItemDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default) => repository.GetByIdAsync(id, cancellationToken);
    public Task<int> CreateAsync(MenuItemUpsertRequest request, CancellationToken cancellationToken = default) => repository.CreateAsync(request, cancellationToken);
    public Task<bool> UpdateAsync(int id, MenuItemUpsertRequest request, CancellationToken cancellationToken = default) => repository.UpdateAsync(id, request, cancellationToken);
    public Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default) => repository.DeleteAsync(id, cancellationToken);
}
