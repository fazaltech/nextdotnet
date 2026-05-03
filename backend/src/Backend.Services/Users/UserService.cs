using Backend.Application.Users;
using Backend.Domain.Identity;
using Microsoft.AspNetCore.Identity;

namespace Backend.Services.Users;

public sealed class UserService(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager) : IUserService
{
    public async Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = userManager.Users.OrderBy(x => x.Id).ToList();
        var result = new List<UserDto>(users.Count);

        foreach (var user in users)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var roles = await userManager.GetRolesAsync(user);
            result.Add(new UserDto(user.Id, user.UserName ?? string.Empty, user.Email ?? string.Empty, user.FullName, user.IsActive, roles.ToArray()));
        }

        return result;
    }

    public async Task<UserDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null)
        {
            return null;
        }

        var roles = await userManager.GetRolesAsync(user);
        return new UserDto(user.Id, user.UserName ?? string.Empty, user.Email ?? string.Empty, user.FullName, user.IsActive, roles.ToArray());
    }

    public async Task<int> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Roles.Count > 0)
        {
            foreach (var role in request.Roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    throw new InvalidOperationException($"Role '{role}' does not exist.");
                }
            }
        }

        var user = new ApplicationUser
        {
            UserName = request.UserName,
            Email = request.Email,
            FullName = request.FullName,
            IsActive = true,
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", createResult.Errors.Select(x => x.Description)));
        }

        if (request.Roles.Count > 0)
        {
            var addRoleResult = await userManager.AddToRolesAsync(user, request.Roles);
            if (!addRoleResult.Succeeded)
            {
                throw new InvalidOperationException(string.Join("; ", addRoleResult.Errors.Select(x => x.Description)));
            }
        }

        return user.Id;
    }

    public async Task<bool> UpdateAsync(int id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null)
        {
            return false;
        }

        user.FullName = request.FullName;
        user.Email = request.Email;
        user.IsActive = request.IsActive;

        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", updateResult.Errors.Select(x => x.Description)));
        }

        return true;
    }

    public async Task<bool> AssignRolesAsync(int id, AssignRolesRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null)
        {
            return false;
        }

        foreach (var role in request.Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                throw new InvalidOperationException($"Role '{role}' does not exist.");
            }
        }

        var currentRoles = await userManager.GetRolesAsync(user);
        if (currentRoles.Count > 0)
        {
            var removeResult = await userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                throw new InvalidOperationException(string.Join("; ", removeResult.Errors.Select(x => x.Description)));
            }
        }

        if (request.Roles.Count > 0)
        {
            var addResult = await userManager.AddToRolesAsync(user, request.Roles);
            if (!addResult.Succeeded)
            {
                throw new InvalidOperationException(string.Join("; ", addResult.Errors.Select(x => x.Description)));
            }
        }

        return true;
    }
}
