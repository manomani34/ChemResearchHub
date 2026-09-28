using ChemResearchHub.Application.WorkItemTransitions.Dtos;
using ChemResearchHub.Application.WorkItemTransitions.Interfaces;
using ChemResearchHub.Application.WorkItemTransitions.Repositories;

namespace ChemResearchHub.Application.WorkItemTransitions.Services;

public class WorkItemFlowSummaryService : IWorkItemFlowSummaryService
{
    private readonly IWorkItemTransitionRepository _repository;

    public WorkItemFlowSummaryService(
        IWorkItemTransitionRepository repository)
    {
        _repository = repository;
    }

    public async Task<WorkItemFlowSummaryDto> CalculateAsync(
        int boardId,
        DateTime periodStartUtc,
        DateTime periodEndUtc,
        CancellationToken cancellationToken = default)
    {
        if (boardId <= 0)
            return Empty(boardId, periodStartUtc, periodEndUtc);

        if (periodEndUtc <= periodStartUtc)
            return Empty(boardId, periodStartUtc, periodEndUtc);

        var transitions =
            await _repository.GetByBoardIdAsync(
                boardId,
                cancellationToken);

        var completedItems = new List<WorkItemFlowSummaryItemDto>();

        foreach (var group in transitions.GroupBy(x => x.WorkItemId))
        {
            var ordered = group
                .OrderBy(x => x.ChangedAtUtc)
                .ThenBy(x => x.Id)
                .ToList();

            if (ordered.Count == 0 || !ordered[^1].IsCompleted)
                continue;

            var createdAtUtc = ordered[0].ChangedAtUtc;
            var completedAtUtc = ordered[^1].ChangedAtUtc;

            if (completedAtUtc < periodStartUtc || completedAtUtc >= periodEndUtc)
                continue;

            var leadTime =
                completedAtUtc >= createdAtUtc
                    ? completedAtUtc - createdAtUtc
                    : (TimeSpan?)null;

            var cycleStartedAtUtc =
                ordered.Count > 1
                    ? ordered[1].ChangedAtUtc
                    : (DateTime?)null;

            var cycleTime =
                cycleStartedAtUtc.HasValue &&
                completedAtUtc >= cycleStartedAtUtc.Value
                    ? completedAtUtc - cycleStartedAtUtc.Value
                    : (TimeSpan?)null;

            completedItems.Add(new WorkItemFlowSummaryItemDto
            {
                WorkItemId = ordered[^1].WorkItemId,
                Title = ordered[^1].WorkItemTitle,
                CompletedAtUtc = completedAtUtc,
                LeadTime = leadTime,
                CycleTime = cycleTime
            });
        }

        completedItems = completedItems
            .OrderByDescending(x => x.CompletedAtUtc)
            .ThenBy(x => x.WorkItemId)
            .ToList();

        return new WorkItemFlowSummaryDto
        {
            BoardId = boardId,
            PeriodStartUtc = periodStartUtc,
            PeriodEndUtc = periodEndUtc,
            Throughput = completedItems.Count,
            AverageLeadTime = Average(completedItems.Select(x => x.LeadTime)),
            MedianLeadTime = Median(completedItems.Select(x => x.LeadTime)),
            AverageCycleTime = Average(completedItems.Select(x => x.CycleTime)),
            MedianCycleTime = Median(completedItems.Select(x => x.CycleTime)),
            CompletedItems = completedItems
        };
    }

    private static WorkItemFlowSummaryDto Empty(
        int boardId,
        DateTime periodStartUtc,
        DateTime periodEndUtc)
    {
        return new WorkItemFlowSummaryDto
        {
            BoardId = boardId,
            PeriodStartUtc = periodStartUtc,
            PeriodEndUtc = periodEndUtc,
            Throughput = 0
        };
    }

    private static TimeSpan? Average(IEnumerable<TimeSpan?> values)
    {
        var ticks = values
            .Where(x => x.HasValue)
            .Select(x => x!.Value.Ticks)
            .ToList();

        if (ticks.Count == 0)
            return null;

        return TimeSpan.FromTicks(
            (long)ticks.Average());
    }

    private static TimeSpan? Median(IEnumerable<TimeSpan?> values)
    {
        var ticks = values
            .Where(x => x.HasValue)
            .Select(x => x!.Value.Ticks)
            .OrderBy(x => x)
            .ToList();

        if (ticks.Count == 0)
            return null;

        var middle = ticks.Count / 2;

        return ticks.Count % 2 == 1
            ? TimeSpan.FromTicks(ticks[middle])
            : TimeSpan.FromTicks(
                (long)(((double)ticks[middle - 1] + ticks[middle]) / 2d));
    }
}
