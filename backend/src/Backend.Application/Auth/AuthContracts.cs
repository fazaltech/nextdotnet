namespace Backend.Application.Auth;

public sealed record LoginRequest(string UserNameOrEmail, string Password);

public sealed record LoginResponse(
    string AccessToken,
    DateTime ExpiresAtUtc,
    int UserId,
    string UserName,
    string Email,
    string FullName,
    IReadOnlyList<string> Roles);

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}

public interface IJwtTokenGenerator
{
    Task<LoginResponse> GenerateAsync(int userId, string userName, string email, string fullName, IReadOnlyList<string> roles, CancellationToken cancellationToken = default);
}
