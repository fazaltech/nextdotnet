using Backend.Application.Expenses;
using Backend.Application.Persistence;

namespace Backend.Services.Expenses;

public sealed class ExpenseService(IExpenseRepository repository) : IExpenseService
{
    public Task<IReadOnlyList<ExpenseDto>> GetAllAsync(CancellationToken cancellationToken = default) => repository.GetAllAsync(cancellationToken);
    public Task<ExpenseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default) => repository.GetByIdAsync(id, cancellationToken);
    public Task<int> CreateAsync(ExpenseUpsertRequest request, CancellationToken cancellationToken = default) => repository.CreateAsync(request, cancellationToken);
    public Task<bool> UpdateAsync(int id, ExpenseUpsertRequest request, CancellationToken cancellationToken = default) => repository.UpdateAsync(id, request, cancellationToken);
    public Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default) => repository.DeleteAsync(id, cancellationToken);
}
