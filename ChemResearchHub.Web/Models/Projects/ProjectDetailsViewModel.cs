namespace ChemResearchHub.Web.Models.Projects;

public class ProjectDetailsViewModel
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string? Description { get; init; }

    public bool IsActive { get; init; }
}