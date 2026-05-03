using Backend.Application.Tables;

namespace Backend.Application.Persistence;

public interface ITableRepository
{
    Task<IReadOnlyList<DiningTableDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<DiningTableDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(DiningTableUpsertRequest request, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(int id, DiningTableUpsertRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> SetOccupiedAsync(int id, bool isOccupied, CancellationToken cancellationToken = default);
}
