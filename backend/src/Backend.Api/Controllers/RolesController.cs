using Backend.Application.Roles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/roles")]
public sealed class RolesController(IRolePermissionService rolePermissionService) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<RoleDto>> GetRoles(CancellationToken cancellationToken)
        => rolePermissionService.GetRolesAsync(cancellationToken);

    [HttpPost]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request, CancellationToken cancellationToken)
    {
        var id = await rolePermissionService.CreateRoleAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetRoles), new { id }, new { id });
    }

    [HttpGet("permissions")]
    public Task<IReadOnlyList<PermissionDto>> GetPermissions(CancellationToken cancellationToken)
        => rolePermissionService.GetPermissionsAsync(cancellationToken);

    [HttpGet("{roleId:int}/permissions")]
    public Task<IReadOnlyList<int>> GetPermissionsByRole(int roleId, CancellationToken cancellationToken)
        => rolePermissionService.GetPermissionsByRoleIdAsync(roleId, cancellationToken);

    [HttpPut("{roleId:int}/permissions")]
    public async Task<IActionResult> UpdatePermissions(int roleId, [FromBody] UpdateRolePermissionsRequest request, CancellationToken cancellationToken)
    {
        var ok = await rolePermissionService.UpdateRolePermissionsAsync(roleId, request, cancellationToken);
        return ok ? NoContent() : NotFound();
    }
}
