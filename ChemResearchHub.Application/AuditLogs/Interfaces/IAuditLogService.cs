using ChemResearchHub.Application.AuditLogs.Dtos;

namespace ChemResearchHub.Application.AuditLogs.Interfaces;

public interface IAuditLogService
{
    Task<IReadOnlyList<AuditLogDto>> GetAsync(
        string? search = null,
        string? entityType = null,
        string? action = null,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        string? userId,
        string action,
        string entityType,
        int? entityId,
        string? description,
        string? metadata = null,
        CancellationToken cancellationToken = default);
}
