namespace ChemResearchHub.Web.Authorization.Permissions;

public static class PermissionCatalog
{
    public static IReadOnlyList<PermissionDefinition> All { get; } =
    [
        new("Users.View", "View users", "Users", "View user accounts."),
        new("Users.Create", "Create users", "Users", "Create user accounts."),
        new("Users.Edit", "Edit users", "Users", "Edit user information."),
        new("Users.Delete", "Delete users", "Users", "Delete user accounts."),

        new("Roles.View", "View roles", "Roles", "View application roles."),
        new("Roles.Create", "Create roles", "Roles", "Create application roles."),
        new("Roles.Edit", "Edit roles", "Roles", "Edit application roles."),
        new("Roles.Delete", "Delete roles", "Roles", "Delete application roles."),
        new("Roles.ManagePermissions", "Manage role permissions", "Roles",
            "Assign or remove permissions from roles."),

        new("Projects.View", "View projects", "Projects", "View research projects."),
        new("Projects.Create", "Create projects", "Projects", "Create research projects."),
        new("Projects.Edit", "Edit projects", "Projects", "Edit research projects."),
        new("Projects.Delete", "Delete projects", "Projects", "Delete research projects."),

        new("Experiments.View", "View experiments", "Experiments", "View experiments."),
        new("Experiments.Create", "Create experiments", "Experiments", "Create experiments."),
        new("Experiments.Edit", "Edit experiments", "Experiments", "Edit experiments."),
        new("Experiments.Delete", "Delete experiments", "Experiments", "Delete experiments."),

        new("Samples.View", "View samples", "Samples", "View research samples."),
        new("Samples.Create", "Create samples", "Samples", "Register research samples."),
        new("Samples.Edit", "Edit samples", "Samples", "Edit research samples."),
        new("Samples.Delete", "Delete samples", "Samples", "Delete research samples."),

        new("Results.View", "View results", "Results", "View research results."),
        new("Results.Create", "Create results", "Results", "Register research results."),
        new("Results.Edit", "Edit results", "Results", "Edit research results."),
        new("Results.Delete", "Delete results", "Results", "Delete research results."),

        new("ResearchReviews.View", "View reviews", "Research Reviews", "View research reviews."),
        new("ResearchReviews.Submit", "Submit for review", "Research Reviews", "Submit work for review."),
        new("ResearchReviews.Review", "Review submissions", "Research Reviews", "Review submitted work."),
        new("ResearchReviews.Approve", "Approve submissions", "Research Reviews", "Approve submitted work."),

                new("DecisionLogs.View", "View decision logs", "Decision Logs",
            "View research decision logs."),
        new("DecisionLogs.Create", "Create decision logs", "Decision Logs",
            "Record research decisions."),
        new("DecisionLogs.Edit", "Edit decision logs", "Decision Logs",
            "Edit research decisions."),
        new("DecisionLogs.Delete", "Delete decision logs", "Decision Logs",
            "Delete research decisions."),
    ];

    public static bool Exists(string permission) =>
        All.Any(x => string.Equals(
            x.Name,
            permission,
            StringComparison.OrdinalIgnoreCase));

    public static PermissionDefinition? Find(string permission) =>
        All.FirstOrDefault(x => string.Equals(
            x.Name,
            permission,
            StringComparison.OrdinalIgnoreCase));
}
