using Backend.Application.Abstractions;

namespace Backend.Application.FoodCategories;

public sealed record FoodCategoryDto(int Id, string Name, string? Description, bool IsActive);
public sealed record FoodCategoryUpsertRequest(string Name, string? Description, bool IsActive);

public interface IFoodCategoryService : ICrudService<FoodCategoryDto, FoodCategoryUpsertRequest, FoodCategoryUpsertRequest>;
