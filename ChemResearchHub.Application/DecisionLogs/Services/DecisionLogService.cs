using ChemResearchHub.Application.DecisionLogs.Dtos;
using ChemResearchHub.Application.DecisionLogs.Interfaces;
using ChemResearchHub.Application.DecisionLogs.Repositories;

namespace ChemResearchHub.Application.DecisionLogs.Services;

public class DecisionLogService : IDecisionLogService
{
    private readonly IDecisionLogRepository _repository;

    public DecisionLogService(
        IDecisionLogRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<DecisionLogDto>> GetAllAsync(
    CancellationToken cancellationToken = default)
    {
        return await _repository.GetAllAsync(
            cancellationToken);
    }

    public async Task<IReadOnlyList<DecisionLogDto>> GetByWorkItemIdAsync(
        int workItemId,
        CancellationToken cancellationToken = default)
    {
        if (workItemId <= 0)
        {
            return Array.Empty<DecisionLogDto>();
        }

        return await _repository.GetByWorkItemIdAsync(
            workItemId,
            cancellationToken);
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
        if (workItemId <= 0)
        {
            throw new ArgumentException(
                "WorkItemId must be greater than zero.",
                nameof(workItemId));
        }

        if (string.IsNullOrWhiteSpace(decisionType))
        {
            throw new ArgumentException(
                "Decision type is required.",
                nameof(decisionType));
        }

        if (string.IsNullOrWhiteSpace(decision))
        {
            throw new ArgumentException(
                "Decision is required.",
                nameof(decision));
        }

        return await _repository.CreateAsync(
            workItemId,
            decisionType.Trim(),
            decision.Trim(),
            rationale,
            evidence,
            createdByUserId,
            cancellationToken);
    }
}