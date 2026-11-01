using ChemResearchHub.Application.WorkItemUserAlerts.Repositories;
using ChemResearchHub.Domain.Entities.WorkItemUserAlert;
using ChemResearchHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChemResearchHub.Infrastructure.Persistence.Repositories;

public class WorkItemUserAlertRepository
    : IWorkItemUserAlertRepository
{
    private readonly ApplicationDbContext _dbContext;

    public WorkItemUserAlertRepository(
        ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> GetUnreadCountAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.WorkItemUserAlerts
            .AsNoTracking()
            .CountAsync(
                x => x.UserId == userId && x.ReadAt == null,
                cancellationToken);
    }

    public async Task AddOrReactivateRangeAsync(
        int workItemId,
        IReadOnlyCollection<string> userIds,
        CancellationToken cancellationToken = default)
    {
        if (workItemId <= 0 || userIds.Count == 0)
            return;

        var normalizedUserIds = userIds
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (normalizedUserIds.Count == 0)
            return;

        var existingAlerts = await _dbContext.WorkItemUserAlerts
            .Where(x =>
                x.WorkItemId == workItemId &&
                normalizedUserIds.Contains(x.UserId))
            .ToListAsync(cancellationToken);

        var existingUserIds = existingAlerts
            .Select(x => x.UserId)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var alert in existingAlerts)
        {
            alert.MarkAsUnread();
        }

        var newAlerts = normalizedUserIds
            .Where(userId => !existingUserIds.Contains(userId))
            .Select(userId =>
                new WorkItemUserAlert(workItemId, userId))
            .ToList();

        if (newAlerts.Count > 0)
        {
            await _dbContext.WorkItemUserAlerts.AddRangeAsync(
                newAlerts,
                cancellationToken);
        }
    }

    public async Task MarkAllAsReadAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var unreadAlerts = await _dbContext.WorkItemUserAlerts
            .Where(x => x.UserId == userId && x.ReadAt == null)
            .ToListAsync(cancellationToken);

        foreach (var alert in unreadAlerts)
            alert.MarkAsRead();
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}