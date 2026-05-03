namespace Backend.Application.Roles;

public sealed record RoleDto(int Id, string Name, string? Description);
public sealed record PermissionDto(int Id, string Name, string Description);
public sealed record CreateRoleRequest(string Name, string? Description);
public sealed record UpdateRolePermissionsRequest(IReadOnlyList<int> PermissionIds);

public interface IRolePermissionService
{
    Task<IReadOnlyList<RoleDto>> GetRolesAsync(CancellationToken cancellationToken = default);
    Task<int> CreateRoleAsync(CreateRoleRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PermissionDto>> GetPermissionsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<int>> GetPermissionsByRoleIdAsync(int roleId, CancellationToken cancellationToken = default);
    Task<bool> UpdateRolePermissionsAsync(int roleId, UpdateRolePermissionsRequest request, CancellationToken cancellationToken = default);
}
