namespace Backend.Application.Users;

public sealed record UserDto(int Id, string UserName, string Email, string FullName, bool IsActive, IReadOnlyList<string> Roles);
public sealed record CreateUserRequest(string UserName, string Email, string FullName, string Password, IReadOnlyList<string> Roles);
public sealed record UpdateUserRequest(string FullName, string Email, bool IsActive);
public sealed record AssignRolesRequest(IReadOnlyList<string> Roles);

public interface IUserService
{
    Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<UserDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(int id, UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task<bool> AssignRolesAsync(int id, AssignRolesRequest request, CancellationToken cancellationToken = default);
}
