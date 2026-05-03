using Backend.Application.Orders;
using Backend.Application.Persistence;

namespace Backend.Services.Orders;

public sealed class OrderService(IOrderRepository repository) : IOrderService
{
    public Task<IReadOnlyList<OrderDto>> GetAllAsync(CancellationToken cancellationToken = default) => repository.GetAllAsync(cancellationToken);
    public Task<OrderDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default) => repository.GetByIdAsync(id, cancellationToken);
    public Task<int> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken = default) => repository.CreateAsync(request, cancellationToken);
    public Task<bool> UpdateStatusAsync(int id, UpdateOrderStatusRequest request, CancellationToken cancellationToken = default)
        => repository.UpdateStatusAsync(id, request.Status, cancellationToken);
}
