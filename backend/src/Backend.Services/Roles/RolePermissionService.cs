using Backend.Application.Persistence;
using Backend.Application.Roles;
using Backend.Domain.Identity;
using Microsoft.AspNetCore.Identity;

namespace Backend.Services.Roles;

public sealed class RolePermissionService(
    RoleManager<ApplicationRole> roleManager,
    IRolePermissionRepository rolePermissionRepository) : IRolePermissionService
{
    public Task<IReadOnlyList<PermissionDto>> GetPermissionsAsync(CancellationToken cancellationToken = default)
        => rolePermissionRepository.GetPermissionsAsync(cancellationToken);

    public Task<IReadOnlyList<int>> GetPermissionsByRoleIdAsync(int roleId, CancellationToken cancellationToken = default)
        => rolePermissionRepository.GetPermissionsByRoleIdAsync(roleId, cancellationToken);

    public async Task<IReadOnlyList<RoleDto>> GetRolesAsync(CancellationToken cancellationToken = default)
    {
        var roles = roleManager.Roles
            .OrderBy(x => x.Name)
            .Select(x => new RoleDto(x.Id, x.Name ?? string.Empty, x.Description))
            .ToArray();

        return roles;
    }

    public async Task<int> CreateRoleAsync(CreateRoleRequest request, CancellationToken cancellationToken = default)
    {
        var exists = await roleManager.RoleExistsAsync(request.Name);
        if (exists)
        {
            throw new InvalidOperationException($"Role '{request.Name}' already exists.");
        }

        var role = new ApplicationRole
        {
            Name = request.Name,
            Description = request.Description
        };

        var result = await roleManager.CreateAsync(role);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(x => x.Description)));
        }

        return role.Id;
    }

    public async Task<bool> UpdateRolePermissionsAsync(int roleId, UpdateRolePermissionsRequest request, CancellationToken cancellationToken = default)
    {
        var role = await roleManager.FindByIdAsync(roleId.ToString());
        if (role is null)
        {
            return false;
        }

        await rolePermissionRepository.UpdateRolePermissionsAsync(roleId, request.PermissionIds, cancellationToken);
        return true;
    }
}
