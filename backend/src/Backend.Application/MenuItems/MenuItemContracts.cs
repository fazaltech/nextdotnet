using Backend.Application.Abstractions;

namespace Backend.Application.MenuItems;

public sealed record MenuItemDto(int Id, int CategoryId, string CategoryName, string Name, string? Description, decimal Price, bool IsAvailable, bool IsActive);
public sealed record MenuItemUpsertRequest(int CategoryId, string Name, string? Description, decimal Price, bool IsAvailable, bool IsActive);

public interface IMenuItemService : ICrudService<MenuItemDto, MenuItemUpsertRequest, MenuItemUpsertRequest>;
