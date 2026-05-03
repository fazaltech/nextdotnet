using Backend.Application.MenuItems;

namespace Backend.Application.Persistence;

public interface IMenuItemRepository
{
    Task<IReadOnlyList<MenuItemDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<MenuItemDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(MenuItemUpsertRequest request, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(int id, MenuItemUpsertRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
