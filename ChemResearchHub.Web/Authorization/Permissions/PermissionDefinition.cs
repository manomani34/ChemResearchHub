namespace ChemResearchHub.Web.Authorization.Permissions;

public sealed record PermissionDefinition(
    string Name,
    string DisplayName,
    string Group,
    string Description);
