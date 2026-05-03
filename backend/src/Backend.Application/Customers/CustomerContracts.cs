using Backend.Application.Abstractions;

namespace Backend.Application.Customers;

public sealed record CustomerDto(int Id, string FullName, string? Phone, string? Email, bool IsActive);
public sealed record CustomerUpsertRequest(string FullName, string? Phone, string? Email, bool IsActive);

public interface ICustomerService : ICrudService<CustomerDto, CustomerUpsertRequest, CustomerUpsertRequest>;
