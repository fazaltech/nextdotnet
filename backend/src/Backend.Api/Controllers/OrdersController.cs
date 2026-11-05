using Backend.Application.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/orders")]
public sealed class OrdersController(IOrderService service, Backend.Api.ServiceBridge bridge) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<OrderDto>> GetAll(CancellationToken cancellationToken) => service.GetAllAsync(cancellationToken);

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var row = await service.GetByIdAsync(id, cancellationToken);
        return row is null ? NotFound() : Ok(row);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var sessionId=await bridge.SessionForTable(request.TableId,cancellationToken);
        var id = await bridge.Run("submit-order",new Backend.Application.Operations.OperationRequest { TableId=request.TableId,SessionId=sessionId,CustomerId=request.CustomerId,Items=request.Items.Select(x=>new Backend.Application.Operations.OperationLine(x.MenuItemId,x.Quantity)).ToArray() },cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusRequest request, CancellationToken cancellationToken)
    {
        if(request.Status!=Backend.Domain.Enums.OrderStatus.InProgress)
            throw new InvalidOperationException("Use item-level service actions for preparation, delivery, or cancellation with a reason.");
        await bridge.Run("approve-order",new Backend.Application.Operations.OperationRequest { Id=id },cancellationToken);
        return NoContent();
    }
}
