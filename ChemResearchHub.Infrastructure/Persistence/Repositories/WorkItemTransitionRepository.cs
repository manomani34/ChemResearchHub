using ChemResearchHub.Application.WorkItemTransitions.Dtos;
using ChemResearchHub.Application.WorkItemTransitions.Repositories;
using ChemResearchHub.Domain.Entities.WorkItemTransition;
using Microsoft.EntityFrameworkCore;

namespace ChemResearchHub.Infrastructure.Persistence.Repositories;

public class WorkItemTransitionRepository : IWorkItemTransitionRepository
{
    private readonly ApplicationDbContext _context;

    public WorkItemTransitionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<WorkItemTransitionDto>> GetByWorkItemIdAsync(
        int workItemId,
        CancellationToken cancellationToken = default)
    {
        return await ProjectQuery()
            .Where(x => x.WorkItemId == workItemId)
            .OrderByDescending(x => x.ChangedAtUtc)
            .ThenByDescending(x => x.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<WorkItemTransitionDto>> GetByBoardIdAsync(
        int boardId,
        CancellationToken cancellationToken = default)
    {
        return await ProjectQuery()
            .Where(x => x.BoardId == boardId)
            .OrderBy(x => x.WorkItemId)
            .ThenBy(x => x.ChangedAtUtc)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        WorkItemTransition transition,
        CancellationToken cancellationToken = default)
    {
        await _context.WorkItemTransitions.AddAsync(
            transition,
            cancellationToken);
    }

    private IQueryable<WorkItemTransitionDto> ProjectQuery()
    {
        return _context.WorkItemTransitions
            .AsNoTracking()
            .Select(x => new WorkItemTransitionDto
            {
                Id = x.Id,
                WorkItemId = x.WorkItemId,
                BoardId = _context.BoardColumns
                    .Where(c => c.Id == x.WorkItem.BoardColumnId)
                    .Select(c => c.BoardId)
                    .FirstOrDefault(),
                WorkItemTitle = x.WorkItem.Title,
                IsCompleted = x.WorkItem.IsCompleted,
                FromBoardColumnId = x.FromBoardColumnId,
                FromBoardColumnName = x.FromBoardColumnId.HasValue
                    ? _context.BoardColumns
                        .Where(c => c.Id == x.FromBoardColumnId.Value)
                        .Select(c => c.Name)
                        .FirstOrDefault()
                    : null,
                ToBoardColumnId = x.ToBoardColumnId,
                ToBoardColumnName = _context.BoardColumns
                    .Where(c => c.Id == x.ToBoardColumnId)
                    .Select(c => c.Name)
                    .FirstOrDefault() ?? string.Empty,
                ChangedByUserId = x.ChangedByUserId,
                ChangedByUserName = x.ChangedByUserId != null
                    ? _context.Users
                        .Where(u => u.Id == x.ChangedByUserId)
                        .Select(u => string.IsNullOrWhiteSpace(u.FullName)
                            ? u.Email
                            : u.FullName)
                        .FirstOrDefault()
                    : null,
                ChangedAtUtc = x.ChangedAtUtc
            });
    }
}
