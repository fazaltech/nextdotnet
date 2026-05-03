using Backend.Application.Expenses;

namespace Backend.Application.Persistence;

public interface IExpenseRepository
{
    Task<IReadOnlyList<ExpenseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ExpenseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(ExpenseUpsertRequest request, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(int id, ExpenseUpsertRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
