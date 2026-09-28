namespace ChemResearchHub.Application.WorkItemTransitions.Dtos;

public class WorkItemFlowSummaryItemDto
{
    public int WorkItemId { get; init; }

    public string Title { get; init; } = string.Empty;

    public DateTime CompletedAtUtc { get; init; }

    public TimeSpan? LeadTime { get; init; }

    public TimeSpan? CycleTime { get; init; }
}
