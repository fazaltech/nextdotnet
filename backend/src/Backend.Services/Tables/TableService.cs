using Backend.Application.Persistence;
using Backend.Application.Tables;

namespace Backend.Services.Tables;

public sealed class TableService(ITableRepository repository) : ITableService
{
    public Task<IReadOnlyList<DiningTableDto>> GetAllAsync(CancellationToken cancellationToken = default) => repository.GetAllAsync(cancellationToken);
    public Task<DiningTableDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default) => repository.GetByIdAsync(id, cancellationToken);
    public Task<int> CreateAsync(DiningTableUpsertRequest request, CancellationToken cancellationToken = default) => repository.CreateAsync(request, cancellationToken);
    public Task<bool> UpdateAsync(int id, DiningTableUpsertRequest request, CancellationToken cancellationToken = default) => repository.UpdateAsync(id, request, cancellationToken);
    public Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default) => repository.DeleteAsync(id, cancellationToken);
}
