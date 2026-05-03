using Backend.Application.Customers;
using Backend.Application.Persistence;

namespace Backend.Services.Customers;

public sealed class CustomerService(ICustomerRepository repository) : ICustomerService
{
    public Task<IReadOnlyList<CustomerDto>> GetAllAsync(CancellationToken cancellationToken = default) => repository.GetAllAsync(cancellationToken);
    public Task<CustomerDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default) => repository.GetByIdAsync(id, cancellationToken);
    public Task<int> CreateAsync(CustomerUpsertRequest request, CancellationToken cancellationToken = default) => repository.CreateAsync(request, cancellationToken);
    public Task<bool> UpdateAsync(int id, CustomerUpsertRequest request, CancellationToken cancellationToken = default) => repository.UpdateAsync(id, request, cancellationToken);
    public Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default) => repository.DeleteAsync(id, cancellationToken);
}
