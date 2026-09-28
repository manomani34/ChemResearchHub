using ChemResearchHub.Application.AuditLogs.Dtos;
using ChemResearchHub.Application.AuditLogs.Repositories;
using ChemResearchHub.Domain.Entities.AuditLog;
using ChemResearchHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChemResearchHub.Infrastructure.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly ApplicationDbContext _context;

    public AuditLogRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<AuditLogDto>> GetAsync(
        string? search,
        string? entityType,
        string? action,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query =
            from log in _context.Set<AuditLog>()
            join user in _context.Users
                on log.UserId equals user.Id into users
            from user in users.DefaultIfEmpty()
            select new AuditLogDto
            {
                Id = log.Id,
                UserId = log.UserId,
                UserName = user == null ? null : user.FullName,
                Action = log.Action,
                EntityType = log.EntityType,
                EntityId = log.EntityId,
                Description = log.Description,
                Metadata = log.Metadata,
                CreatedAtUtc = log.CreatedAt
            };

        if (!string.IsNullOrWhiteSpace(search))
        {
            var value = search.Trim();

            query = query.Where(x =>
                (x.UserName != null && x.UserName.Contains(value)) ||
                x.Action.Contains(value) ||
                x.EntityType.Contains(value) ||
                (x.Description != null && x.Description.Contains(value)) ||
                (x.Metadata != null && x.Metadata.Contains(value)));
        }

        if (!string.IsNullOrWhiteSpace(entityType))
            query = query.Where(x => x.EntityType == entityType);

        if (!string.IsNullOrWhiteSpace(action))
            query = query.Where(x => x.Action == action);

        return await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<AuditLog> AddAsync(
        AuditLog entity,
        CancellationToken cancellationToken = default)
    {
        await _context.Set<AuditLog>().AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
