using Backend.Application.Inventory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/inventory")]
public sealed class InventoryController(IInventoryService service) : ControllerBase
{
    [HttpGet("items")]
    public Task<IReadOnlyList<InventoryItemDto>> GetItems(CancellationToken cancellationToken) => service.GetAllAsync(cancellationToken);

    [HttpGet("items/{id:int}")]
    public async Task<IActionResult> GetItemById(int id, CancellationToken cancellationToken)
    {
        var row = await service.GetByIdAsync(id, cancellationToken);
        return row is null ? NotFound() : Ok(row);
    }

    [HttpPost("items")]
    public async Task<IActionResult> CreateItem([FromBody] InventoryItemUpsertRequest request, CancellationToken cancellationToken)
    {
        var id = await service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetItemById), new { id }, new { id });
    }

    [HttpPut("items/{id:int}")]
    public async Task<IActionResult> UpdateItem(int id, [FromBody] InventoryItemUpsertRequest request, CancellationToken cancellationToken)
    {
        return await service.UpdateAsync(id, request, cancellationToken) ? NoContent() : NotFound();
    }

    [HttpDelete("items/{id:int}")]
    public async Task<IActionResult> DeleteItem(int id, CancellationToken cancellationToken)
    {
        return await service.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();
    }

    [HttpPost("stock-transactions")]
    public async Task<IActionResult> CreateStockTransaction([FromBody] StockTransactionRequest request, CancellationToken cancellationToken)
    {
        var id = await service.CreateStockTransactionAsync(request, cancellationToken);
        return Ok(new { id });
    }
}
