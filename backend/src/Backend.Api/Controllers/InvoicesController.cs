using Backend.Application.Invoices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/invoices")]
public sealed class InvoicesController(IInvoiceService service) : ControllerBase
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
        var id = await service.GenerateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }
}
