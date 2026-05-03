using Backend.Application.Roles;

namespace Backend.Application.Persistence;

public interface IRolePermissionRepository
{
    Task<IReadOnlyList<PermissionDto>> GetPermissionsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<int>> GetPermissionsByRoleIdAsync(int roleId, CancellationToken cancellationToken = default);
    Task UpdateRolePermissionsAsync(int roleId, IReadOnlyList<int> permissionIds, CancellationToken cancellationToken = default);
}
