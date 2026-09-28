using ChemResearchHub.Application.WorkItemTransitions.Dtos;

namespace ChemResearchHub.Application.WorkItemTransitions.Interfaces;

public interface IWorkItemTransitionService
{
    Task<IReadOnlyList<WorkItemTransitionDto>> GetByWorkItemIdAsync(
        int workItemId,
        CancellationToken cancellationToken = default);
}
