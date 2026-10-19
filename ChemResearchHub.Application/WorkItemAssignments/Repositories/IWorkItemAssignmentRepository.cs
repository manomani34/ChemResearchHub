namespace ChemResearchHub.Application.WorkItemAssignments.Repositories;

public interface IWorkItemAssignmentRepository
{
    Task<IReadOnlyList<string>> GetUserIdsByWorkItemIdAsync(
        int workItemId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<int, IReadOnlyList<string>>>
        GetUserIdsByWorkItemIdsAsync(
            IReadOnlyCollection<int> workItemIds,
            CancellationToken cancellationToken = default);

    Task ReplaceAssignmentsAsync(
        int workItemId,
        IReadOnlyCollection<string> userIds,
        string? assignedByUserId = null,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
