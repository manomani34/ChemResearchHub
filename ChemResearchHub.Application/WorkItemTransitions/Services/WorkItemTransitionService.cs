using ChemResearchHub.Application.WorkItemTransitions.Dtos;
using ChemResearchHub.Application.WorkItemTransitions.Interfaces;
using ChemResearchHub.Application.WorkItemTransitions.Repositories;

namespace ChemResearchHub.Application.WorkItemTransitions.Services;

public class WorkItemTransitionService : IWorkItemTransitionService
{
    private readonly IWorkItemTransitionRepository _repository;

    public WorkItemTransitionService(
        IWorkItemTransitionRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<WorkItemTransitionDto>> GetByWorkItemIdAsync(
        int workItemId,
        CancellationToken cancellationToken = default)
    {
        if (workItemId <= 0)
        {
            return Array.Empty<WorkItemTransitionDto>();
        }

        return await _repository.GetByWorkItemIdAsync(
            workItemId,
            cancellationToken);
    }
}
