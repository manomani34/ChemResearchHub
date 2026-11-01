namespace ChemResearchHub.Application.WorkItemUserAlerts.Interfaces;

public interface IWorkItemUserAlertService
{
    Task<int> GetUnreadCountAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task CreateForWorkItemAsync(
        int workItemId,
        string? primaryUserId,
        IReadOnlyCollection<string>? collaboratorIds,
        CancellationToken cancellationToken = default);

    Task MarkAllAsReadAsync(
        string userId,
        CancellationToken cancellationToken = default);
}