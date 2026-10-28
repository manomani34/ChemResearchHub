
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
        var users = await _userManager.Users
            .AsNoTracking()
            .OrderBy(x => x.FullName)
            .ThenBy(x => x.Email)
            .ToListAsync(cancellationToken);

        var result = new List<UserDto>(users.Count);

        foreach (var user in users)
        {
            result.Add(await MapUserAsync(user));
        }

        return result;
    }

    public async Task<IReadOnlyList<UserDto>> GetActiveUsersAsync(
        CancellationToken cancellationToken = default)
    {
        var users = await _userManager.Users
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.FullName)
            .ThenBy(x => x.Email)
            .ToListAsync(cancellationToken);

        var result = new List<UserDto>(users.Count);

        foreach (var user in users)
        {
            result.Add(await MapUserAsync(user));
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

        var user = await _userManager.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == userId,
                cancellationToken);

        return user is null
            ? null
            : await MapUserAsync(user);
    }

    public async Task<bool> UpdateAsync(
        string userId,
        string fullName,
        string? email,
        IReadOnlyCollection<string> roleNames,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId) ||
            string.IsNullOrWhiteSpace(fullName))
        {
            return false;
        }

        var targetRoles = NormalizeRoles(roleNames);

        if (targetRoles.Count == 0)
        {
            throw new InvalidOperationException(
                "At least one role must be selected.");
        }

        await EnsureRolesExistAsync(targetRoles);

        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
        {
            return false;
        }

        user.FullName = fullName.Trim();
        user.IsActive = isActive;

        if (email is not null &&
            !string.Equals(
                user.Email,
                email.Trim(),
                StringComparison.OrdinalIgnoreCase))
        {
            var normalizedEmail = email.Trim();

            var emailResult = await _userManager.SetEmailAsync(
                user,
                normalizedEmail);

            if (!emailResult.Succeeded)
            {
                throw new InvalidOperationException(
                    FormatErrors(emailResult.Errors));
            }

            var userNameResult = await _userManager.SetUserNameAsync(
                user,
                normalizedEmail);

            if (!userNameResult.Succeeded)
            {
                throw new InvalidOperationException(
                    FormatErrors(userNameResult.Errors));
            }
        }

        var currentRoles = await _userManager.GetRolesAsync(user);

        var rolesToRemove = currentRoles
            .Where(role => !targetRoles.Contains(
                role,
                StringComparer.OrdinalIgnoreCase))
            .ToArray();

        if (rolesToRemove.Length > 0)
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(
                user,
                rolesToRemove);

            if (!removeResult.Succeeded)
            {
                throw new InvalidOperationException(
                    FormatErrors(removeResult.Errors));
            }
        }

        var rolesToAdd = targetRoles
            .Where(role => !currentRoles.Contains(
                role,
                StringComparer.OrdinalIgnoreCase))
            .ToArray();

        if (rolesToAdd.Length > 0)
        {
            var addResult = await _userManager.AddToRolesAsync(
                user,
                rolesToAdd);

            if (!addResult.Succeeded)
            {
                throw new InvalidOperationException(
                    FormatErrors(addResult.Errors));
            }
        }

        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            throw new InvalidOperationException(
                FormatErrors(updateResult.Errors));
        }

        var stampResult =
            await _userManager.UpdateSecurityStampAsync(user);

        if (!stampResult.Succeeded)
        {
            throw new InvalidOperationException(
                FormatErrors(stampResult.Errors));
        }

        return true;
    }

    public async Task<UserDto?> CreateAsync(
        string fullName,
        string email,
        string password,
        IReadOnlyCollection<string> roleNames,
        CancellationToken cancellationToken = default)
    {
        fullName = fullName.Trim();
        email = email.Trim();

        if (string.IsNullOrWhiteSpace(fullName) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            return null;
        }

        var targetRoles = NormalizeRoles(roleNames);

        if (targetRoles.Count == 0)
        {
            throw new InvalidOperationException(
                "At least one role must be selected.");
        }

        await EnsureRolesExistAsync(targetRoles);

        var existingUser = await _userManager.FindByEmailAsync(email);

        if (existingUser is not null)
        {
            throw new InvalidOperationException(
                "A user with this email already exists.");
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = fullName,
            IsActive = true
        };

        var createResult = await _userManager.CreateAsync(
            user,
            password);

        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException(
                $"Failed to create user: {FormatErrors(createResult.Errors)}");
        }

        var roleResult = await _userManager.AddToRolesAsync(
            user,
            targetRoles);

        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);

            throw new InvalidOperationException(
                $"Failed to assign roles: {FormatErrors(roleResult.Errors)}");
        }

        return new UserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            IsActive = user.IsActive,
            RoleNames = targetRoles
        };
    }

    private async Task<UserDto> MapUserAsync(
        ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        return new UserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            IsActive = user.IsActive,
            RoleNames = roles.ToArray()
        };
    }

    private async Task EnsureRolesExistAsync(
        IReadOnlyCollection<string> roleNames)
    {
        foreach (var roleName in roleNames)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                throw new InvalidOperationException(
                    $"Role '{roleName}' does not exist.");
            }
        }
    }

    private static List<string> NormalizeRoles(
        IEnumerable<string>? roleNames)
    {
        return (roleNames ?? Array.Empty<string>())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string FormatErrors(
        IEnumerable<IdentityError> errors)
    {
        return string.Join(
            "; ",
            errors.Select(x => x.Description));
    }
}
