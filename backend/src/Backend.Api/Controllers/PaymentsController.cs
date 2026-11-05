using Backend.Application.Payments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/payments")]
public sealed class PaymentsController(IPaymentService service, Backend.Api.ServiceBridge bridge) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<PaymentDto>> GetAll(CancellationToken cancellationToken) => service.GetAllAsync(cancellationToken);

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var row = await service.GetByIdAsync(id, cancellationToken);
        return row is null ? NotFound() : Ok(row);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePaymentRequest request, CancellationToken cancellationToken)
    {
        var id = await bridge.Run("pay",new Backend.Application.Operations.OperationRequest { Id=request.InvoiceId,Amount=request.Amount,Method=(int)request.Method,Reference=request.TransactionReference },cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }
}
