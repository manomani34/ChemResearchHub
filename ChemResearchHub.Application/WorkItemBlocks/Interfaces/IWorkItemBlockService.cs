using ChemResearchHub.Application.WorkItemBlocks.Dtos;

namespace ChemResearchHub.Application.WorkItemBlocks.Interfaces;

public interface IWorkItemBlockService
{
    Task<WorkItemBlockDto?> GetCurrentAsync(int workItemId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WorkItemBlockDto>> GetHistoryAsync(int workItemId, CancellationToken cancellationToken = default);
    Task<bool> BlockAsync(int workItemId, string reason, string? userId, CancellationToken cancellationToken = default);
    Task<bool> UnblockAsync(int workItemId, string? note, string? userId, CancellationToken cancellationToken = default);
}
