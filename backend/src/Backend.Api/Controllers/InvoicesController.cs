using Backend.Application.Invoices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/invoices")]
public sealed class InvoicesController(IInvoiceService service, Backend.Application.Orders.IOrderService orders, Backend.Api.ServiceBridge bridge) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<InvoiceDto>> GetAll(CancellationToken cancellationToken) => service.GetAllAsync(cancellationToken);

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var row = await service.GetByIdAsync(id, cancellationToken);
        return row is null ? NotFound() : Ok(row);
    }

    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody] GenerateInvoiceRequest request, CancellationToken cancellationToken)
    {
        var order=await orders.GetByIdAsync(request.OrderId,cancellationToken);
        if(order is null) return NotFound();
        if(request.TaxAmount!=0) throw new InvalidOperationException("Tax is calculated from the protected TaxPercent setting; do not send a tax amount.");
        var id = await bridge.Run("bill",new Backend.Application.Operations.OperationRequest { SessionId=await bridge.SessionForTable(order.TableId,cancellationToken),ItemIds=order.Items.Select(x=>x.Id).ToArray() },cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }
}
