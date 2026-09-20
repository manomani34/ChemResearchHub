using ChemResearchHub.Application.DecisionLogs.Dtos;
using ChemResearchHub.Application.DecisionLogs.Repositories;
using ChemResearchHub.Domain.Entities.DecisionLog;
using Microsoft.EntityFrameworkCore;

namespace ChemResearchHub.Infrastructure.Persistence.Repositories;

public class DecisionLogRepository : IDecisionLogRepository
{
    private readonly ApplicationDbContext _context;

    public DecisionLogRepository(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<DecisionLogDto>> GetAllAsync(
    CancellationToken cancellationToken = default)
    {
        return await _context.DecisionLogs
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .Select(x => new DecisionLogDto
            {
                Id = x.Id,
                WorkItemId = x.WorkItemId,
                DecisionType = x.DecisionType,
                Decision = x.Decision,
                Rationale = x.Rationale,
                Evidence = x.Evidence,
                CreatedByUserId = x.CreatedByUserId,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DecisionLogDto>> GetByWorkItemIdAsync(
        int workItemId,
        CancellationToken cancellationToken = default)
    {
        return await _context.DecisionLogs
            .AsNoTracking()
            .Where(x => x.WorkItemId == workItemId)
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .Select(x => new DecisionLogDto
            {
                Id = x.Id,
                WorkItemId = x.WorkItemId,
                DecisionType = x.DecisionType,
                Decision = x.Decision,
                Rationale = x.Rationale,
                Evidence = x.Evidence,
                CreatedByUserId = x.CreatedByUserId,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<DecisionLogDto?> CreateAsync(
        int workItemId,
        string decisionType,
        string decision,
        string? rationale,
        string? evidence,
        string? createdByUserId,
        CancellationToken cancellationToken = default)
    {
        var log =
            new DecisionLog(
                workItemId,
                decisionType,
                decision,
                rationale,
                evidence,
                createdByUserId);

        await _context.DecisionLogs.AddAsync(
            log,
            cancellationToken);

        await _context.SaveChangesAsync(
            cancellationToken);

        return new DecisionLogDto
        {
            Id = log.Id,
            WorkItemId = log.WorkItemId,
            DecisionType = log.DecisionType,
            Decision = log.Decision,
            Rationale = log.Rationale,
            Evidence = log.Evidence,
            CreatedByUserId = log.CreatedByUserId,
            CreatedAt = log.CreatedAt
        };
    }
}