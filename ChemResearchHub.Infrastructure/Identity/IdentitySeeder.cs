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

        // Seed roles
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

        // Bootstrap the first Admin user
        var adminUsers =
            await userManager.GetUsersInRoleAsync("Admin");

        if (adminUsers.Count == 0)
        {
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
                    $"Admin role assigned to: {firstUser.Email}");
            }
            else
            {
                Console.WriteLine(
                    "No users found. Admin role will be assigned after the first user is created.");
            }
        }
        else
        {
            Console.WriteLine(
                $"Admin user already exists: {adminUsers[0].Email}");
        }

        Console.WriteLine(
            "Identity role seeding completed.");
    }
}