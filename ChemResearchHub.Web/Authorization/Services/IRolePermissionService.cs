using Microsoft.AspNetCore.Identity;

namespace ChemResearchHub.Web.Authorization.Services;

public interface IRolePermissionService
{
    Task<IReadOnlyCollection<string>> GetPermissionsAsync(
        string roleId);

    Task<IdentityResult> SetPermissionsAsync(
        string roleId,
        IEnumerable<string> permissions);
}
