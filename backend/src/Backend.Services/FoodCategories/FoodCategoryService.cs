using Backend.Application.FoodCategories;
using Backend.Application.Persistence;

namespace Backend.Services.FoodCategories;

public sealed class FoodCategoryService(IFoodCategoryRepository repository) : IFoodCategoryService
{
    public Task<IReadOnlyList<FoodCategoryDto>> GetAllAsync(CancellationToken cancellationToken = default) => repository.GetAllAsync(cancellationToken);
    public Task<FoodCategoryDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default) => repository.GetByIdAsync(id, cancellationToken);
    public Task<int> CreateAsync(FoodCategoryUpsertRequest request, CancellationToken cancellationToken = default) => repository.CreateAsync(request, cancellationToken);
    public Task<bool> UpdateAsync(int id, FoodCategoryUpsertRequest request, CancellationToken cancellationToken = default) => repository.UpdateAsync(id, request, cancellationToken);
    public Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default) => repository.DeleteAsync(id, cancellationToken);
}
