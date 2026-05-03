using Backend.Application.Tables;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/tables")]
public sealed class TablesController(ITableService service) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<DiningTableDto>> GetAll(CancellationToken cancellationToken) => service.GetAllAsync(cancellationToken);

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var row = await service.GetByIdAsync(id, cancellationToken);
        return row is null ? NotFound() : Ok(row);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] DiningTableUpsertRequest request, CancellationToken cancellationToken)
    {
        var id = await service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] DiningTableUpsertRequest request, CancellationToken cancellationToken)
    {
        return await service.UpdateAsync(id, request, cancellationToken) ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        return await service.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();
    }
}
