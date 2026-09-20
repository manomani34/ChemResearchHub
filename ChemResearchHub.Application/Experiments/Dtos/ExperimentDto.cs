namespace ChemResearchHub.Application.Experiments.Dtos;

public class ExperimentDto
{
    public int Id { get; init; }

    public int WorkItemId { get; init; }

    public string Title { get; init; } = string.Empty;

    public string? Description { get; init; }

    public string? Protocol { get; init; }

    public DateTime? StartedAt { get; init; }

    public DateTime? CompletedAt { get; init; }

    public string? Notes { get; init; }
}