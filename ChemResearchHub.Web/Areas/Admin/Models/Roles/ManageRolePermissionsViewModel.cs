using ChemResearchHub.Web.Authorization.Permissions;

namespace ChemResearchHub.Web.Areas.Admin.Models.Roles;

public class ManageRolePermissionsViewModel
{
    public string RoleId { get; set; } = string.Empty;

    public string RoleName { get; set; } = string.Empty;

    public List<RolePermissionGroupViewModel> Groups { get; set; } = [];

    public List<string> SelectedPermissions { get; set; } = [];
}

public class RolePermissionGroupViewModel
{
    public string Name { get; set; } = string.Empty;

    public List<RolePermissionItemViewModel> Permissions { get; set; } = [];
}

public class RolePermissionItemViewModel
{
    public string Name { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsSelected { get; set; }
}
