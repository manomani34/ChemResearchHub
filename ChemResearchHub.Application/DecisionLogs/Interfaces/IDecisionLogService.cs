using ChemResearchHub.Application.DecisionLogs.Dtos;

namespace ChemResearchHub.Application.DecisionLogs.Interfaces;

public interface IDecisionLogService
{
    Task<IReadOnlyList<DecisionLogDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DecisionLogDto>> GetByWorkItemIdAsync(
        int workItemId,
        CancellationToken cancellationToken = default);

    Task<DecisionLogDto?> CreateAsync(
        int workItemId,
        string decisionType,
        string decision,
        string? rationale,
        string? evidence,
        string? createdByUserId,
        CancellationToken cancellationToken = default);
}