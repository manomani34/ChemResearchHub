using ChemResearchHub.Domain.Entities.WorkItemUserAlert;

namespace ChemResearchHub.Application.WorkItemUserAlerts.Repositories;

public interface IWorkItemUserAlertRepository
{
    Task<int> GetUnreadCountAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task AddOrReactivateRangeAsync(
        int workItemId,
        IReadOnlyCollection<string> userIds,
        CancellationToken cancellationToken = default);

    Task MarkAllAsReadAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}