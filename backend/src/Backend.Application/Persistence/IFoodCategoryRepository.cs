using Backend.Application.FoodCategories;

namespace Backend.Application.Persistence;

public interface IFoodCategoryRepository
{
    Task<IReadOnlyList<FoodCategoryDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<FoodCategoryDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(FoodCategoryUpsertRequest request, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(int id, FoodCategoryUpsertRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
