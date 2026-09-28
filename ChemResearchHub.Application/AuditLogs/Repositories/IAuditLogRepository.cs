using ChemResearchHub.Application.AuditLogs.Dtos;
using ChemResearchHub.Domain.Entities.AuditLog;

namespace ChemResearchHub.Application.AuditLogs.Repositories;

public interface IAuditLogRepository
{
    Task<IReadOnlyList<AuditLogDto>> GetAsync(
        string? search,
        string? entityType,
        string? action,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<AuditLog> AddAsync(
        AuditLog entity,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
