using Backend.Application.Auth;
using Backend.Domain.Identity;
using Microsoft.AspNetCore.Identity;

namespace Backend.Services.Auth;

public sealed class AuthService(
    UserManager<ApplicationUser> userManager,
    IJwtTokenGenerator tokenGenerator) : IAuthService
{
    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var normalized = request.UserNameOrEmail.Trim();

        ApplicationUser? user;
        if (normalized.Contains('@'))
        {
            user = await userManager.FindByEmailAsync(normalized);
        }
        else
        {
            user = await userManager.FindByNameAsync(normalized);
        }

        if (user is null || !user.IsActive)
        {
            return null;
        }

        var passwordOk = await userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordOk)
        {
            return null;
        }

        var roles = await userManager.GetRolesAsync(user);

        return await tokenGenerator.GenerateAsync(
            user.Id,
            user.UserName ?? string.Empty,
            user.Email ?? string.Empty,
            user.FullName,
            roles.ToArray(),
            cancellationToken);
    }
}
