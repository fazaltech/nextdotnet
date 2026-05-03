using Backend.Domain.Identity;
using Dapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories.Initialization;

public sealed class DataSeeder(
    RoleManager<ApplicationRole> roleManager,
    UserManager<ApplicationUser> userManager,
    Database.IDbConnectionFactory connectionFactory) : IDataSeeder
{
    private static readonly (string Name, string Description)[] DefaultPermissions =
    [
        ("users.manage", "Manage users"),
        ("roles.manage", "Manage roles and permissions"),
        ("foodcategories.manage", "Manage food categories"),
        ("menuitems.manage", "Manage menu items"),
        ("tables.manage", "Manage dining tables"),
        ("customers.manage", "Manage customers"),
        ("orders.manage", "Manage orders"),
        ("invoices.manage", "Manage billing and invoices"),
        ("payments.manage", "Manage payments"),
        ("inventory.manage", "Manage inventory"),
        ("expenses.manage", "Manage expenses"),
        ("reports.view", "View sales reports"),
        ("dashboard.view", "View dashboard summary"),
        ("settings.manage", "Manage app settings")
    ];

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        const string adminRoleName = "Admin";
        const string managerRoleName = "Manager";

        if (!await roleManager.RoleExistsAsync(adminRoleName))
        {
            await roleManager.CreateAsync(new ApplicationRole { Name = adminRoleName, Description = "System Administrator" });
        }

        if (!await roleManager.RoleExistsAsync(managerRoleName))
        {
            await roleManager.CreateAsync(new ApplicationRole { Name = managerRoleName, Description = "Restaurant Manager" });
        }

        var adminUser = await userManager.Users.FirstOrDefaultAsync(x => x.UserName == "admin", cancellationToken);
        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                UserName = "admin",
                Email = "admin@restaurant.local",
                FullName = "System Admin",
                EmailConfirmed = true,
                IsActive = true
            };

            var createResult = await userManager.CreateAsync(adminUser, "Admin@12345");
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(string.Join("; ", createResult.Errors.Select(x => x.Description)));
            }
        }

        if (!await userManager.IsInRoleAsync(adminUser, adminRoleName))
        {
            await userManager.AddToRoleAsync(adminUser, adminRoleName);
        }

        using var connection = connectionFactory.CreateConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await ((Microsoft.Data.SqlClient.SqlConnection)connection).OpenAsync(cancellationToken);
        }

        const string insertPermissionSql = @"
IF NOT EXISTS (SELECT 1 FROM dbo.RmsPermissions WHERE Name = @Name)
BEGIN
    INSERT INTO dbo.RmsPermissions(Name, Description) VALUES (@Name, @Description);
END";

        foreach (var permission in DefaultPermissions)
        {
            await connection.ExecuteAsync(new CommandDefinition(
                insertPermissionSql,
                new { permission.Name, permission.Description },
                cancellationToken: cancellationToken));
        }

        var adminRole = await roleManager.FindByNameAsync(adminRoleName)
            ?? throw new InvalidOperationException("Admin role was not found after seeding.");

        var permissionIds = (await connection.QueryAsync<int>(new CommandDefinition(
            "SELECT Id FROM dbo.RmsPermissions WHERE IsActive = 1;",
            cancellationToken: cancellationToken))).ToArray();

        const string clearAndInsertSql = @"
DELETE FROM dbo.RmsRolePermissions WHERE RoleId = @RoleId;
INSERT INTO dbo.RmsRolePermissions(RoleId, PermissionId)
SELECT @RoleId, p.Id
FROM dbo.RmsPermissions p
WHERE p.Id IN @PermissionIds;";

        await connection.ExecuteAsync(new CommandDefinition(
            clearAndInsertSql,
            new { RoleId = adminRole.Id, PermissionIds = permissionIds },
            cancellationToken: cancellationToken));
    }
}

