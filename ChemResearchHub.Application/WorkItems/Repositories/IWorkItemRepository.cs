using ChemResearchHub.Domain.Entities.WorkItem;

namespace ChemResearchHub.Application.WorkItems.Repositories;

public interface IWorkItemRepository
{
    Task<IReadOnlyList<WorkItem>> GetByBoardColumnIdAsync(
        int boardColumnId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WorkItem>> GetAllAsync(
        string? search = null,
        bool? isCompleted = null,
        string? assignedToUserId = null,
        CancellationToken cancellationToken = default);

    Task<WorkItem?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        WorkItem workItem,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}