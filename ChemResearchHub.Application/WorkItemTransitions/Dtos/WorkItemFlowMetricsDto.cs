namespace ChemResearchHub.Application.WorkItemTransitions.Dtos;

public class WorkItemFlowMetricsDto
{
    public DateTime? CreatedAtUtc { get; init; }

    public DateTime? CurrentColumnEnteredAtUtc { get; init; }

    public DateTime? CompletedAtUtc { get; init; }

    public DateTime? CycleStartedAtUtc { get; init; }

    public TimeSpan? Age { get; init; }

    public TimeSpan? CurrentColumnAge { get; init; }

    public TimeSpan? FlowTime { get; init; }

    public TimeSpan? LeadTime { get; init; }

    public TimeSpan? CycleTime { get; init; }

    public int TransitionCount { get; init; }

    public bool IsCompleted { get; init; }
}
