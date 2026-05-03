using Backend.Application.Customers;

namespace Backend.Application.Persistence;

public interface ICustomerRepository
{
    Task<IReadOnlyList<CustomerDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CustomerDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(CustomerUpsertRequest request, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(int id, CustomerUpsertRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
