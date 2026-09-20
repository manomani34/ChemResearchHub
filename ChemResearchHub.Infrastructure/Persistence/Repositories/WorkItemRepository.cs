using ChemResearchHub.Application.WorkItems.Repositories;
using ChemResearchHub.Domain.Entities.WorkItem;
using Microsoft.EntityFrameworkCore;

namespace ChemResearchHub.Infrastructure.Persistence.Repositories;

public class WorkItemRepository : IWorkItemRepository
{
    private readonly ApplicationDbContext _dbContext;

    public WorkItemRepository(
        ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<WorkItem>> GetByBoardColumnIdAsync(
        int boardColumnId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.WorkItems
            .AsNoTracking()
            .Where(x =>
                x.BoardColumnId == boardColumnId)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);
    }


    public async Task<IReadOnlyList<WorkItem>> GetAllAsync(
        string? search = null,
        bool? isCompleted = null,
        string? assignedToUserId = null,
        CancellationToken cancellationToken = default)
    {
        var query =
            _dbContext.WorkItems
                .AsNoTracking()
                .AsQueryable();


        if (!string.IsNullOrWhiteSpace(search))
        {
            search =
                search.Trim();

            query =
                query.Where(
                    x =>
                        x.Title.Contains(search) ||
                        (x.Description != null &&
                         x.Description.Contains(search)));
        }


        if (isCompleted.HasValue)
        {
            query =
                query.Where(
                    x =>
                        x.IsCompleted ==
                        isCompleted.Value);
        }


        if (!string.IsNullOrWhiteSpace(assignedToUserId))
        {
            query =
                query.Where(
                    x =>
                        x.AssignedToUserId ==
                        assignedToUserId);
        }


        return await query
            .OrderBy(x => x.IsCompleted)
            .ThenBy(x => x.DueDate)
            .ThenBy(x => x.Priority)
            .ThenByDescending(x => x.Id)
            .ToListAsync(cancellationToken);
    }


    public async Task<WorkItem?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.WorkItems
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }


    public async Task AddAsync(
        WorkItem workItem,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.WorkItems.AddAsync(
            workItem,
            cancellationToken);
    }


    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }
}