using ChemResearchHub.Application.Users.Dtos;
using ChemResearchHub.Application.Users.Repositories;
using ChemResearchHub.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ChemResearchHub.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UserRepository(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }


    public async Task<IReadOnlyList<UserDto>> GetAllAsync(
    CancellationToken cancellationToken = default)
    {
        var users =
            await _userManager.Users
                .AsNoTracking()
                .OrderBy(x => x.FullName)
                .ThenBy(x => x.Email)
                .ToListAsync(cancellationToken);

        var result =
            new List<UserDto>();

        foreach (var user in users)
        {
            var roles =
                await _userManager.GetRolesAsync(user);

            result.Add(
                new UserDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    IsActive = user.IsActive,
                    RoleName = roles.FirstOrDefault() ?? string.Empty
                });
        }

        return result;
    }


    public async Task<IReadOnlyList<UserDto>> GetActiveUsersAsync(
        CancellationToken cancellationToken = default)
    {
        var users =
            await _userManager.Users
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.FullName)
                .ThenBy(x => x.Email)
                .ToListAsync(cancellationToken);

        var result =
            new List<UserDto>();

        foreach (var user in users)
        {
            var roles =
                await _userManager.GetRolesAsync(user);

            result.Add(
                new UserDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    IsActive = user.IsActive,
                    RoleName = roles.FirstOrDefault() ?? string.Empty
                });
        }

        return result;
    }

    public async Task<UserDto?> GetByIdAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        var user =
            await _userManager.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == userId,
                    cancellationToken);

        if (user is null)
        {
            return null;
        }

        var roles =
            await _userManager.GetRolesAsync(user);

        return new UserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            IsActive = user.IsActive,
            RoleName = roles.FirstOrDefault() ?? string.Empty
        };
    }

    public async Task<bool> UpdateAsync(
        string userId,
        string fullName,
        string? email,
        string roleName,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(roleName))
        {
            return false;
        }

        var user =
            await _userManager.FindByIdAsync(userId);

        if (user is null)
        {
            return false;
        }

        var roleExists =
            await _roleManager.RoleExistsAsync(
                roleName.Trim());

        if (!roleExists)
        {
            return false;
        }

        user.FullName =
            fullName.Trim();

        user.IsActive =
            isActive;

        if (email is not null &&
            !string.Equals(
                user.Email,
                email.Trim(),
                StringComparison.OrdinalIgnoreCase))
        {
            var normalizedEmail =
                email.Trim();

            var emailResult =
                await _userManager.SetEmailAsync(
                    user,
                    normalizedEmail);

            if (!emailResult.Succeeded)
            {
                return false;
            }

            var userNameResult =
                await _userManager.SetUserNameAsync(
                    user,
                    normalizedEmail);

            if (!userNameResult.Succeeded)
            {
                return false;
            }
        }

        var currentRoles =
            await _userManager.GetRolesAsync(user);

        var targetRole =
            roleName.Trim();

        var rolesToRemove =
            currentRoles
                .Where(
                    role =>
                        !string.Equals(
                            role,
                            targetRole,
                            StringComparison.OrdinalIgnoreCase))
                .ToList();

        if (rolesToRemove.Count > 0)
        {
            var removeResult =
                await _userManager.RemoveFromRolesAsync(
                    user,
                    rolesToRemove);

            if (!removeResult.Succeeded)
            {
                return false;
            }
        }

        var hasTargetRole =
            currentRoles.Any(
                role =>
                    string.Equals(
                        role,
                        targetRole,
                        StringComparison.OrdinalIgnoreCase));

        if (!hasTargetRole)
        {
            var addResult =
                await _userManager.AddToRoleAsync(
                    user,
                    targetRole);

            if (!addResult.Succeeded)
            {
                return false;
            }
        }

        var securityStampResult =
    await _userManager.UpdateSecurityStampAsync(user);

        if (!securityStampResult.Succeeded)
        {
            return false;
        }

        var updateResult =
            await _userManager.UpdateAsync(user);

        return updateResult.Succeeded;
    }

    public async Task<UserDto?> CreateAsync(
        string fullName,
        string email,
        string password,
        string roleName,
        CancellationToken cancellationToken = default)
    {
        fullName = fullName.Trim();
        email = email.Trim();
        roleName = roleName.Trim();

        if (string.IsNullOrWhiteSpace(fullName) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(roleName))
        {
            return null;
        }

        var roleExists =
            await _roleManager.RoleExistsAsync(
                roleName);

        if (!roleExists)
        {
            throw new InvalidOperationException(
                $"Role '{roleName}' does not exist.");
        }

        var existingUser =
            await _userManager.FindByEmailAsync(email);

        if (existingUser is not null)
        {
            throw new InvalidOperationException(
                "A user with this email already exists.");
        }

        var user =
            new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                IsActive = true
            };

        var createResult =
            await _userManager.CreateAsync(
                user,
                password);

        if (!createResult.Succeeded)
        {
            var errors =
                string.Join(
                    "; ",
                    createResult.Errors.Select(
                        x => x.Description));

            throw new InvalidOperationException(
                $"Failed to create user: {errors}");
        }

        var roleResult =
            await _userManager.AddToRoleAsync(
                user,
                roleName);

        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);

            var errors =
                string.Join(
                    "; ",
                    roleResult.Errors.Select(
                        x => x.Description));

            throw new InvalidOperationException(
                $"Failed to assign role '{roleName}': {errors}");
        }

        return new UserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            IsActive = user.IsActive,
            RoleName = roleName
        };
    }
}