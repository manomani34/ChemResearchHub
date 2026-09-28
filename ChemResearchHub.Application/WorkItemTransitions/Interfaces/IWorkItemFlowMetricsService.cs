using ChemResearchHub.Application.WorkItemTransitions.Dtos;

namespace ChemResearchHub.Application.WorkItemTransitions.Interfaces;

public interface IWorkItemFlowMetricsService
{
    Task<WorkItemFlowMetricsDto> CalculateAsync(
        int workItemId,
        bool isCompleted,
        CancellationToken cancellationToken = default);
}
