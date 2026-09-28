namespace ChemResearchHub.Application.WorkItemTransitions.Dtos;

public class WorkItemFlowSummaryDto
{
    public int BoardId { get; init; }

    public DateTime PeriodStartUtc { get; init; }

    public DateTime PeriodEndUtc { get; init; }

    public int Throughput { get; init; }

    public TimeSpan? AverageLeadTime { get; init; }

    public TimeSpan? MedianLeadTime { get; init; }

    public TimeSpan? AverageCycleTime { get; init; }

    public TimeSpan? MedianCycleTime { get; init; }

    public IReadOnlyList<WorkItemFlowSummaryItemDto> CompletedItems { get; init; }
        = Array.Empty<WorkItemFlowSummaryItemDto>();
}
