using ChemResearchHub.Application.AuditLogs.Interfaces;
using ChemResearchHub.Application.WorkItemBlocks.Dtos;
using ChemResearchHub.Application.WorkItemBlocks.Interfaces;
using ChemResearchHub.Application.WorkItemBlocks.Repositories;

namespace ChemResearchHub.Application.WorkItemBlocks.Services;

public class WorkItemBlockService : IWorkItemBlockService
{
    private readonly IWorkItemBlockRepository _repository;
    private readonly IAuditLogService _auditLogService;

    public WorkItemBlockService(
        IWorkItemBlockRepository repository,
        IAuditLogService auditLogService)
    {
        _repository = repository;
        _auditLogService = auditLogService;
    }

    public Task<WorkItemBlockDto?> GetCurrentAsync(
        int workItemId,
        CancellationToken cancellationToken = default)
        => _repository.GetCurrentAsync(
            workItemId,
            cancellationToken);

    public Task<IReadOnlyList<WorkItemBlockDto>> GetHistoryAsync(
        int workItemId,
        CancellationToken cancellationToken = default)
        => _repository.GetHistoryAsync(
            workItemId,
            cancellationToken);

    public async Task<bool> BlockAsync(
        int workItemId,
        string reason,
        string? userId,
        CancellationToken cancellationToken = default)
    {
        if (workItemId <= 0 || string.IsNullOrWhiteSpace(reason))
        {
            return false;
        }

        if (await _repository.GetCurrentAsync(
                workItemId,
                cancellationToken) is not null)
        {
            return false;
        }

        var result = await _repository.AddAsync(
            workItemId,
            reason,
            userId,
            cancellationToken);

        if (!result)
        {
            return false;
        }

        await _auditLogService.AddAsync(
            userId,
            "Blocked",
            "WorkItem",
            workItemId,
            $"Work Item #{workItemId} was blocked. Reason: {reason}",
            cancellationToken: cancellationToken);

        return true;
    }

    public async Task<bool> UnblockAsync(
        int workItemId,
        string? note,
        string? userId,
        CancellationToken cancellationToken = default)
    {
        if (workItemId <= 0)
        {
            return false;
        }

        var current = await _repository.GetCurrentAsync(
            workItemId,
            cancellationToken);

        if (current is null)
        {
            return false;
        }

        var entity = await _repository.GetEntityByIdAsync(
            current.Id,
            cancellationToken);

        if (entity is null)
        {
            return false;
        }

        entity.Unblock(
            userId,
            note);

        await _repository.SaveChangesAsync(
            cancellationToken);

        await _auditLogService.AddAsync(
            userId,
            "Unblocked",
            "WorkItem",
            workItemId,
            string.IsNullOrWhiteSpace(note)
                ? $"Work Item #{workItemId} was unblocked."
                : $"Work Item #{workItemId} was unblocked. Note: {note}",
            cancellationToken: cancellationToken);

        return true;
    }
}