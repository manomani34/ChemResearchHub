using ChemResearchHub.Application.WorkItemBlocks.Dtos;
using ChemResearchHub.Domain.Entities.WorkItemBlock;

namespace ChemResearchHub.Application.WorkItemBlocks.Repositories;

public interface IWorkItemBlockRepository
{
    Task<WorkItemBlockDto?> GetCurrentAsync(int workItemId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WorkItemBlockDto>> GetHistoryAsync(int workItemId, CancellationToken cancellationToken = default);
    Task<WorkItemBlock?> GetEntityByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> AddAsync(int workItemId, string reason, string? blockedByUserId, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
