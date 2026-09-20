using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChemResearchHub.Infrastructure.Identity;

public static class IdentitySeeder
{
    private static readonly string[] Roles =
    {
        "Admin",
        "Researcher",
        "Reviewer"
    };

    public static async Task SeedAsync(
        IServiceProvider serviceProvider)
    {
        var roleManager =
            serviceProvider.GetRequiredService<
                RoleManager<IdentityRole>>();

        var userManager =
            serviceProvider.GetRequiredService<
                UserManager<ApplicationUser>>();

        // =========================================================
        // Seed Roles
        // =========================================================

        foreach (var roleName in Roles)
        {
            var roleExists =
                await roleManager.RoleExistsAsync(
                    roleName);

            if (!roleExists)
            {
                var result =
                    await roleManager.CreateAsync(
                        new IdentityRole(roleName));

                if (!result.Succeeded)
                {
                    var errors =
                        string.Join(
                            "; ",
                            result.Errors.Select(
                                x => x.Description));

                    throw new InvalidOperationException(
                        $"Failed to create role '{roleName}': {errors}");
                }

                Console.WriteLine(
                    $"Identity Role Created: {roleName}");
            }
            else
            {
                Console.WriteLine(
                    $"Identity Role Already Exists: {roleName}");
            }
        }

        // =========================================================
        // Bootstrap Admin
        // =========================================================

        var adminUsers =
            await userManager.GetUsersInRoleAsync(
                "Admin");

        if (adminUsers.Count > 0)
        {
            Console.WriteLine(
                $"Admin user already exists: {adminUsers[0].Email}");

            return;
        }

        // ---------------------------------------------------------
        // If users already exist, preserve existing behavior:
        // assign the first user to Admin.
        // ---------------------------------------------------------

        var firstUser =
            await userManager.Users
                .OrderBy(x => x.Id)
                .FirstOrDefaultAsync();

        if (firstUser is not null)
        {
            var result =
                await userManager.AddToRoleAsync(
                    firstUser,
                    "Admin");

            if (!result.Succeeded)
            {
                var errors =
                    string.Join(
                        "; ",
                        result.Errors.Select(
                            x => x.Description));

                throw new InvalidOperationException(
                    $"Failed to assign Admin role to '{firstUser.Email}': {errors}");
            }

            Console.WriteLine(
                $"Admin role assigned to existing user: {firstUser.Email}");

            return;
        }

        // =========================================================
        // No users exist -> create the bootstrap Admin
        // =========================================================

        var adminEmail =
            Environment.GetEnvironmentVariable(
                "CHEM_ADMIN_EMAIL");

        var adminPassword =
            Environment.GetEnvironmentVariable(
                "CHEM_ADMIN_PASSWORD");

        var adminFullName =
            Environment.GetEnvironmentVariable(
                "CHEM_ADMIN_FULLNAME");

        if (string.IsNullOrWhiteSpace(adminEmail) ||
            string.IsNullOrWhiteSpace(adminPassword))
        {
            Console.WriteLine(
                "No users found and bootstrap Admin credentials were not provided.");

            Console.WriteLine(
                "Set CHEM_ADMIN_EMAIL and CHEM_ADMIN_PASSWORD before starting the application.");

            return;
        }

        var user =
            new ApplicationUser
            {
                UserName = adminEmail.Trim(),
                Email = adminEmail.Trim(),
                FullName =
                    string.IsNullOrWhiteSpace(adminFullName)
                        ? "System Administrator"
                        : adminFullName.Trim(),
                IsActive = true
            };

        var createResult =
            await userManager.CreateAsync(
                user,
                adminPassword);

        if (!createResult.Succeeded)
        {
            var errors =
                string.Join(
                    "; ",
                    createResult.Errors.Select(
                        x => x.Description));

            throw new InvalidOperationException(
                $"Failed to create bootstrap Admin: {errors}");
        }

        var roleResult =
            await userManager.AddToRoleAsync(
                user,
                "Admin");

        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);

            var errors =
                string.Join(
                    "; ",
                    roleResult.Errors.Select(
                        x => x.Description));

            throw new InvalidOperationException(
                $"Failed to assign Admin role: {errors}");
        }

        Console.WriteLine(
            $"Bootstrap Admin created successfully: {user.Email}");

        Console.WriteLine(
            "Identity role seeding completed.");
    }
}