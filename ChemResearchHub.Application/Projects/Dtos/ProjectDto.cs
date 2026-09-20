namespace ChemResearchHub.Application.Projects.Dtos;

public class ProjectDto
{
    public int Id { get; init; }

    public string Name { get; init; } = null!;

    public string? Description { get; init; }

    public bool IsActive { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime? ModifiedAt { get; init; }
}