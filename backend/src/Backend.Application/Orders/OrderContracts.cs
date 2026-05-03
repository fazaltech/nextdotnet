using Backend.Domain.Enums;

namespace Backend.Application.Orders;

public sealed record CreateOrderItemRequest(int MenuItemId, int Quantity);
public sealed record CreateOrderRequest(int TableId, int? CustomerId, IReadOnlyList<CreateOrderItemRequest> Items);
public sealed record UpdateOrderStatusRequest(OrderStatus Status);
public sealed record OrderItemDto(int Id, int MenuItemId, string MenuItemName, int Quantity, decimal UnitPrice, decimal LineTotal);
public sealed record OrderDto(
    int Id,
    int TableId,
    string TableNumber,
    int? CustomerId,
    string? CustomerName,
    DateTime OrderDateUtc,
    OrderStatus Status,
    decimal Subtotal,
    decimal TaxAmount,
    decimal TotalAmount,
    IReadOnlyList<OrderItemDto> Items);

public interface IOrderService
{
    Task<IReadOnlyList<OrderDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<OrderDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken = default);
    Task<bool> UpdateStatusAsync(int id, UpdateOrderStatusRequest request, CancellationToken cancellationToken = default);
}
