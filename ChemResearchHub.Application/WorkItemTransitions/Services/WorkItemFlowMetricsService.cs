using ChemResearchHub.Application.WorkItemTransitions.Dtos;
using ChemResearchHub.Application.WorkItemTransitions.Interfaces;
using ChemResearchHub.Application.WorkItemTransitions.Repositories;

namespace ChemResearchHub.Application.WorkItemTransitions.Services;

public class WorkItemFlowMetricsService : IWorkItemFlowMetricsService
{
    private readonly IWorkItemTransitionRepository _repository;

    public WorkItemFlowMetricsService(
        IWorkItemTransitionRepository repository)
    {
        _repository = repository;
    }

    public async Task<WorkItemFlowMetricsDto> CalculateAsync(
        int workItemId,
        bool isCompleted,
        CancellationToken cancellationToken = default)
    {
        if (workItemId <= 0)
            return new WorkItemFlowMetricsDto();

        var transitions =
            await _repository.GetByWorkItemIdAsync(
                workItemId,
                cancellationToken);

        if (transitions.Count == 0)
            return new WorkItemFlowMetricsDto();

        var ordered = transitions
            .OrderBy(x => x.ChangedAtUtc)
            .ThenBy(x => x.Id)
            .ToList();

        var createdAtUtc = ordered[0].ChangedAtUtc;
        var currentColumnEnteredAtUtc = ordered[^1].ChangedAtUtc;
        var nowUtc = DateTime.UtcNow;

        var completedAtUtc =
            isCompleted
                ? currentColumnEnteredAtUtc
                : (DateTime?)null;

        // Cycle Time starts when the work item first moves away from
        // its initial workflow column. Lead Time starts at creation.
        var cycleStartedAtUtc =
            ordered.Count > 1
                ? ordered[1].ChangedAtUtc
                : (DateTime?)null;

        var age =
            nowUtc >= createdAtUtc
                ? nowUtc - createdAtUtc
                : TimeSpan.Zero;

        var currentColumnAge =
            nowUtc >= currentColumnEnteredAtUtc
                ? nowUtc - currentColumnEnteredAtUtc
                : TimeSpan.Zero;

        var flowEndUtc =
            isCompleted
                ? currentColumnEnteredAtUtc
                : nowUtc;

        var flowTime =
            flowEndUtc >= createdAtUtc
                ? flowEndUtc - createdAtUtc
                : TimeSpan.Zero;

        var leadTime =
            completedAtUtc.HasValue && completedAtUtc.Value >= createdAtUtc
                ? completedAtUtc.Value - createdAtUtc
                : (TimeSpan?)null;

        var cycleTime =
            completedAtUtc.HasValue &&
            cycleStartedAtUtc.HasValue &&
            completedAtUtc.Value >= cycleStartedAtUtc.Value
                ? completedAtUtc.Value - cycleStartedAtUtc.Value
                : (TimeSpan?)null;

        return new WorkItemFlowMetricsDto
        {
            CreatedAtUtc = createdAtUtc,
            CurrentColumnEnteredAtUtc = currentColumnEnteredAtUtc,
            CompletedAtUtc = completedAtUtc,
            CycleStartedAtUtc = cycleStartedAtUtc,
            Age = age,
            CurrentColumnAge = currentColumnAge,
            FlowTime = flowTime,
            LeadTime = leadTime,
            CycleTime = cycleTime,
            TransitionCount = transitions.Count,
            IsCompleted = isCompleted
        };
    }
}
