using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using ChemResearchHub.Infrastructure.Identity;
using ChemResearchHub.Web.Authorization.Permissions;

namespace ChemResearchHub.Web.Authorization.Permissions;

public sealed class PermissionAuthorizationHandler
    : AuthorizationHandler<PermissionRequirement>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public PermissionAuthorizationHandler(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
            return;

        var user = await _userManager.GetUserAsync(context.User);

        if (user is null || !user.IsActive)
            return;

        var roleNames = await _userManager.GetRolesAsync(user);

        foreach (var roleName in roleNames)
        {
            var role = await _roleManager.FindByNameAsync(roleName);

            if (role is null)
                continue;

            var claims = await _roleManager.GetClaimsAsync(role);

            var hasPermission = claims.Any(claim =>
                claim.Type == PermissionConstants.ClaimType &&
                string.Equals(
                    claim.Value,
                    requirement.Permission,
                    StringComparison.OrdinalIgnoreCase));

            if (hasPermission)
            {
                context.Succeed(requirement);
                return;
            }
        }
    }
}

