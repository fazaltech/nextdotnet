using Backend.Application.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/users")]
public sealed class UsersController(IUserService userService) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<UserDto>> GetAll(CancellationToken cancellationToken)
        => userService.GetAllAsync(cancellationToken);

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var user = await userService.GetByIdAsync(id, cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        var id = await userService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var updated = await userService.UpdateAsync(id, request, cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    [HttpPut("{id:int}/roles")]
    public async Task<IActionResult> AssignRoles(int id, [FromBody] AssignRolesRequest request, CancellationToken cancellationToken)
    {
        var updated = await userService.AssignRolesAsync(id, request, cancellationToken);
        return updated ? NoContent() : NotFound();
    }
}
