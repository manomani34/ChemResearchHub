
namespace ChemResearchHub.Web.Areas.Admin.Models.Roles;

public static class SystemRoles
{
    public const string Admin = "Admin";
    public const string Researcher = "Researcher";
    public const string Reviewer = "Reviewer";

    public static readonly IReadOnlySet<string> All =
        new HashSet<string>(
            new[]
            {
                Admin,
                Researcher,
                Reviewer
            },
            StringComparer.OrdinalIgnoreCase);

    public static bool IsSystemRole(string? roleName)
    {
        return !string.IsNullOrWhiteSpace(roleName)
               && All.Contains(roleName);
    }
}
