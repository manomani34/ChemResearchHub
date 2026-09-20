namespace ChemResearchHub.Application.DecisionLogs.Dtos;

public class DecisionLogDto
{
    public int Id { get; init; }

    public int WorkItemId { get; init; }

    public string DecisionType { get; init; } = string.Empty;

    public string Decision { get; init; } = string.Empty;

    public string? Rationale { get; init; }

    public string? Evidence { get; init; }

    public string? CreatedByUserId { get; init; }

    public DateTime CreatedAt { get; init; }
}