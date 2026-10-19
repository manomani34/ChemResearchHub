using ChemResearchHub.Application.WorkItemAssignments.Repositories;
using ChemResearchHub.Domain.Entities.WorkItemAssignment;
using ChemResearchHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChemResearchHub.Infrastructure.Persistence.Repositories;

public class WorkItemAssignmentRepository : IWorkItemAssignmentRepository
{
    private readonly ApplicationDbContext _dbContext;

    public WorkItemAssignmentRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyDictionary<int, IReadOnlyList<string>>>
        GetUserIdsByWorkItemIdsAsync(
            IReadOnlyCollection<int> workItemIds,
            CancellationToken cancellationToken = default)
    {
        if (workItemIds.Count == 0)
        {
            return new Dictionary<int, IReadOnlyList<string>>();
        }

        var assignments = await _dbContext.WorkItemAssignments
            .AsNoTracking()
            .Where(x => workItemIds.Contains(x.WorkItemId))
            .OrderBy(x => x.UserId)
            .Select(x => new
            {
                x.WorkItemId,
                x.UserId
            })
            .ToListAsync(cancellationToken);

        return assignments
            .GroupBy(x => x.WorkItemId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<string>)group
                    .Select(x => x.UserId)
                    .ToList());
    }


    public async Task<IReadOnlyList<string>> GetUserIdsByWorkItemIdAsync(
        int workItemId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.WorkItemAssignments
            .AsNoTracking()
            .Where(x => x.WorkItemId == workItemId)
            .OrderBy(x => x.UserId)
            .Select(x => x.UserId)
            .ToListAsync(cancellationToken);
    }

    public async Task ReplaceAssignmentsAsync(
        int workItemId,
        IReadOnlyCollection<string> userIds,
        string? assignedByUserId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedUserIds = userIds
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToHashSet(StringComparer.Ordinal);

        var existingAssignments = await _dbContext.WorkItemAssignments
            .Where(x => x.WorkItemId == workItemId)
            .ToListAsync(cancellationToken);

        var existingUserIds = existingAssignments
            .Select(x => x.UserId)
            .ToHashSet(StringComparer.Ordinal);

        var assignmentsToRemove = existingAssignments
            .Where(x => !normalizedUserIds.Contains(x.UserId))
            .ToList();

        _dbContext.WorkItemAssignments.RemoveRange(assignmentsToRemove);

        var assignmentsToAdd = normalizedUserIds
            .Where(userId => !existingUserIds.Contains(userId))
            .Select(userId => new WorkItemAssignment(
                workItemId,
                userId,
                assignedByUserId))
            .ToList();

        await _dbContext.WorkItemAssignments.AddRangeAsync(
            assignmentsToAdd,
            cancellationToken);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
