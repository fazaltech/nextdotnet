using Backend.Application.Abstractions;

namespace Backend.Application.Expenses;

public sealed record ExpenseDto(int Id, string Title, string? Category, decimal Amount, DateTime ExpenseDateUtc, string? Notes, bool IsActive);
public sealed record ExpenseUpsertRequest(string Title, string? Category, decimal Amount, DateTime ExpenseDateUtc, string? Notes, bool IsActive);

public interface IExpenseService : ICrudService<ExpenseDto, ExpenseUpsertRequest, ExpenseUpsertRequest>;
