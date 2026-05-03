namespace Backend.Application.Abstractions;

public interface ICrudService<TDto, in TCreateRequest, in TUpdateRequest>
{
    Task<IReadOnlyList<TDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(TCreateRequest request, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(int id, TUpdateRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
