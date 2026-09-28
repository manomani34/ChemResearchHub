using ChemResearchHub.Application.WorkItemBlocks.Dtos;
using ChemResearchHub.Application.WorkItemBlocks.Repositories;
using ChemResearchHub.Domain.Entities.WorkItemBlock;
using Microsoft.EntityFrameworkCore;

namespace ChemResearchHub.Infrastructure.Persistence.Repositories;

public class WorkItemBlockRepository : IWorkItemBlockRepository
{
    private readonly ApplicationDbContext _context;

    public WorkItemBlockRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<WorkItemBlockDto?> GetCurrentAsync(int workItemId, CancellationToken cancellationToken = default)
    {
        return await Project(_context.WorkItemBlocks.AsNoTracking())
            .Where(x => x.WorkItemId == workItemId && x.UnblockedAtUtc == null)
            .OrderByDescending(x => x.BlockedAtUtc)
            .ThenByDescending(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<WorkItemBlockDto>> GetHistoryAsync(int workItemId, CancellationToken cancellationToken = default)
    {
        return await Project(_context.WorkItemBlocks.AsNoTracking())
            .Where(x => x.WorkItemId == workItemId)
            .OrderByDescending(x => x.BlockedAtUtc)
            .ThenByDescending(x => x.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<WorkItemBlock?> GetEntityByIdAsync(int id, CancellationToken cancellationToken = default)
        => _context.WorkItemBlocks.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<bool> AddAsync(int workItemId, string reason, string? blockedByUserId, CancellationToken cancellationToken = default)
    {
        var workItem = await _context.WorkItems.FirstOrDefaultAsync(x => x.Id == workItemId, cancellationToken);
        if (workItem is null) return false;

        await _context.WorkItemBlocks.AddAsync(
            new WorkItemBlock(workItem, reason, blockedByUserId),
            cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);

    private IQueryable<WorkItemBlockDto> Project(IQueryable<WorkItemBlock> query)
        => query.Select(x => new WorkItemBlockDto
        {
            Id = x.Id,
            WorkItemId = x.WorkItemId,
            Reason = x.Reason,
            BlockedByUserId = x.BlockedByUserId,
            BlockedByUserName = x.BlockedByUserId != null
                ? _context.Users.Where(u => u.Id == x.BlockedByUserId).Select(u => string.IsNullOrWhiteSpace(u.FullName) ? u.Email : u.FullName).FirstOrDefault()
                : null,
            BlockedAtUtc = x.BlockedAtUtc,
            UnblockedByUserId = x.UnblockedByUserId,
            UnblockedByUserName = x.UnblockedByUserId != null
                ? _context.Users.Where(u => u.Id == x.UnblockedByUserId).Select(u => string.IsNullOrWhiteSpace(u.FullName) ? u.Email : u.FullName).FirstOrDefault()
                : null,
            UnblockedAtUtc = x.UnblockedAtUtc,
            UnblockNote = x.UnblockNote
        });
}
