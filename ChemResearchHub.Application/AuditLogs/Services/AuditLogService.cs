using ChemResearchHub.Application.AuditLogs.Dtos;
using ChemResearchHub.Application.AuditLogs.Interfaces;
using ChemResearchHub.Application.AuditLogs.Repositories;
using ChemResearchHub.Domain.Entities.AuditLog;

namespace ChemResearchHub.Application.AuditLogs.Services;

public class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _repository;

    public AuditLogService(IAuditLogRepository repository)
    {
        _repository = repository;
    }

    public Task<IReadOnlyList<AuditLogDto>> GetAsync(
        string? search = null,
        string? entityType = null,
        string? action = null,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        return _repository.GetAsync(
            search,
            entityType,
            action,
            Math.Max(1, page),
            Math.Clamp(pageSize, 1, 200),
            cancellationToken);
    }

    public async Task AddAsync(
        string? userId,
        string action,
        string entityType,
        int? entityId,
        string? description,
        string? metadata = null,
        CancellationToken cancellationToken = default)
    {
        var entity = new AuditLog(
            userId,
            action,
            entityType,
            entityId,
            description,
            metadata);

        await _repository.AddAsync(entity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }
}
