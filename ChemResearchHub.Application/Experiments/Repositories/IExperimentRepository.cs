using ChemResearchHub.Application.Experiments.Dtos;

namespace ChemResearchHub.Application.Experiments.Repositories;

public interface IExperimentRepository
{
    Task<IReadOnlyList<ExperimentDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ExperimentDto>> GetByWorkItemIdAsync(
        int workItemId,
        CancellationToken cancellationToken = default);

    Task<ExperimentDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<ExperimentDto?> CreateAsync(
        int workItemId,
        string title,
        string? description,
        string? protocol,
        DateTime? startedAt,
        DateTime? completedAt,
        string? notes,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(
        int id,
        string title,
        string? description,
        string? protocol,
        DateTime? startedAt,
        DateTime? completedAt,
        string? notes,
        CancellationToken cancellationToken = default);
}