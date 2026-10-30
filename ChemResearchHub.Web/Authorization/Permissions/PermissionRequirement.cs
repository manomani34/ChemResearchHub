using Microsoft.AspNetCore.Authorization;

namespace ChemResearchHub.Web.Authorization.Permissions;

public sealed class PermissionRequirement(string permission)
    : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}
