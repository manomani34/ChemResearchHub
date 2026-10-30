using System.Security.Claims;
using ChemResearchHub.Web.Authorization.Permissions;
using Microsoft.AspNetCore.Identity;

namespace ChemResearchHub.Web.Authorization.Services;

public sealed class RolePermissionService : IRolePermissionService
{
    private readonly RoleManager<IdentityRole> _roleManager;

    public RolePermissionService(
        RoleManager<IdentityRole> roleManager)
    {
        _roleManager = roleManager;
    }

    public async Task<IReadOnlyCollection<string>> GetPermissionsAsync(
        string roleId)
    {
        var role = await _roleManager.FindByIdAsync(roleId);

        if (role is null)
            return Array.Empty<string>();

        var claims = await _roleManager.GetClaimsAsync(role);

        return claims
            .Where(x => x.Type == PermissionConstants.ClaimType)
            .Select(x => x.Value)
            .Where(PermissionCatalog.Exists)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public async Task<IdentityResult> SetPermissionsAsync(
        string roleId,
        IEnumerable<string> permissions)
    {
        var role = await _roleManager.FindByIdAsync(roleId);

        if (role is null)
        {
            return IdentityResult.Failed(new IdentityError
            {
                Description = "The specified role was not found."
            });
        }

        var requestedPermissions = permissions
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var invalidPermissions = requestedPermissions
            .Where(x => !PermissionCatalog.Exists(x))
            .ToArray();

        if (invalidPermissions.Length > 0)
        {
            return IdentityResult.Failed(invalidPermissions
                .Select(x => new IdentityError
                {
                    Description = $"Unknown permission: {x}"
                })
                .ToArray());
        }

        var normalizedPermissions = requestedPermissions
            .Select(x => PermissionCatalog.Find(x)!.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var existingClaims = await _roleManager.GetClaimsAsync(role);

        var existingPermissionClaims = existingClaims
            .Where(x => x.Type == PermissionConstants.ClaimType)
            .ToArray();

        // Remove permissions that are no longer selected.
        foreach (var claim in existingPermissionClaims)
        {
            if (normalizedPermissions.Contains(claim.Value))
                continue;

            var removeResult = await _roleManager.RemoveClaimAsync(
                role,
                claim);

            if (!removeResult.Succeeded)
                return removeResult;
        }

        // Add newly selected permissions.
        var existingPermissionValues = existingPermissionClaims
            .Select(x => x.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var permission in normalizedPermissions)
        {
            if (existingPermissionValues.Contains(permission))
                continue;

            var addResult = await _roleManager.AddClaimAsync(
                role,
                new Claim(
                    PermissionConstants.ClaimType,
                    permission));

            if (!addResult.Succeeded)
                return addResult;
        }

        return IdentityResult.Success;
    }
}
