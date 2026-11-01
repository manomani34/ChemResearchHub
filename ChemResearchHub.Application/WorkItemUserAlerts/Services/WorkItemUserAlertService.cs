using ChemResearchHub.Application.WorkItemUserAlerts.Interfaces;
using ChemResearchHub.Application.WorkItemUserAlerts.Repositories;

namespace ChemResearchHub.Application.WorkItemUserAlerts.Services;

public class WorkItemUserAlertService
    : IWorkItemUserAlertService
{
    private readonly IWorkItemUserAlertRepository _repository;

    public WorkItemUserAlertService(
        IWorkItemUserAlertRepository repository)
    {
        _repository = repository;
    }

    public Task<int> GetUnreadCountAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return _repository.GetUnreadCountAsync(
            userId,
            cancellationToken);
    }

    public async Task CreateForWorkItemAsync(
        int workItemId,
        string? primaryUserId,
        IReadOnlyCollection<string>? collaboratorIds,
        CancellationToken cancellationToken = default)
    {
        var userIds = new HashSet<string>(
            StringComparer.Ordinal);

        if (!string.IsNullOrWhiteSpace(primaryUserId))
            userIds.Add(primaryUserId.Trim());

        if (collaboratorIds is not null)
        {
            foreach (var userId in collaboratorIds)
            {
                if (!string.IsNullOrWhiteSpace(userId))
                    userIds.Add(userId.Trim());
            }
        }

        if (userIds.Count == 0)
            return;

        await _repository.AddOrReactivateRangeAsync(
            workItemId,
            userIds,
            cancellationToken);

        await _repository.SaveChangesAsync(
            cancellationToken);
    }

    public async Task MarkAllAsReadAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        await _repository.MarkAllAsReadAsync(
            userId,
            cancellationToken);

        await _repository.SaveChangesAsync(
            cancellationToken);
    }
}