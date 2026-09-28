using ChemResearchHub.Application.WorkItemTransitions.Dtos;
using ChemResearchHub.Domain.Entities.WorkItemTransition;

namespace ChemResearchHub.Application.WorkItemTransitions.Repositories;

public interface IWorkItemTransitionRepository
{
    Task<IReadOnlyList<WorkItemTransitionDto>> GetByWorkItemIdAsync(
        int workItemId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WorkItemTransitionDto>> GetByBoardIdAsync(
        int boardId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        WorkItemTransition transition,
        CancellationToken cancellationToken = default);
}
