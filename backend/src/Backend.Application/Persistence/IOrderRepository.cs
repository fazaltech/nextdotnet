using Backend.Application.Orders;
using Backend.Domain.Enums;

namespace Backend.Application.Persistence;

public interface IOrderRepository
{
    Task<IReadOnlyList<OrderDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<OrderDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken = default);
    Task<bool> UpdateStatusAsync(int id, OrderStatus status, CancellationToken cancellationToken = default);
}
