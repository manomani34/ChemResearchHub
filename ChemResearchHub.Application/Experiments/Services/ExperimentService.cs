using ChemResearchHub.Application.Experiments.Dtos;
using ChemResearchHub.Application.Experiments.Interfaces;
using ChemResearchHub.Application.Experiments.Repositories;

namespace ChemResearchHub.Application.Experiments.Services;

public class ExperimentService : IExperimentService
{
    private readonly IExperimentRepository _experimentRepository;

    public ExperimentService(
        IExperimentRepository experimentRepository)
    {
        _experimentRepository = experimentRepository;
    }

    public async Task<ExperimentStatistics> GetStatisticsByProjectIdAsync(
    int projectId,
    CancellationToken cancellationToken = default)
    {
        if (projectId <= 0)
        {
            return new ExperimentStatistics();
        }

        return await _experimentRepository
            .GetStatisticsByProjectIdAsync(
                projectId,
                cancellationToken);
    }

    public async Task<ExperimentStatistics> GetStatisticsAsync(
    CancellationToken cancellationToken = default)
    {
        return await _experimentRepository.GetStatisticsAsync(
            cancellationToken);
    }

    public async Task<IReadOnlyList<ExperimentDto>> SearchAsync(
    string searchTerm,
    CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return Array.Empty<ExperimentDto>();
        }

        return await _experimentRepository.SearchAsync(
            searchTerm.Trim(),
            cancellationToken);
    }

    public async Task<ExperimentDto?> GetByTitleAsync(
    string title,
    CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return null;
        }

        return await _experimentRepository.GetByTitleAsync(
            title.Trim(),
            cancellationToken);
    }


    public async Task<ExperimentDto?> GetLatestAsync(
    CancellationToken cancellationToken = default)
    {
        return await _experimentRepository.GetLatestAsync(
            cancellationToken);
    }


    public async Task<IReadOnlyList<ExperimentDto>> GetByProjectIdAsync(
    int projectId,
    CancellationToken cancellationToken = default)
    {
        if (projectId <= 0)
        {
            return Array.Empty<ExperimentDto>();
        }

        return await _experimentRepository.GetByProjectIdAsync(
            projectId,
            cancellationToken);
    }

    public async Task<IReadOnlyList<ExperimentDto>> GetAllAsync(
    CancellationToken cancellationToken = default)
    {
        return await _experimentRepository.GetAllAsync(
            cancellationToken);
    }

    public async Task<IReadOnlyList<ExperimentDto>> GetByWorkItemIdAsync(
        int workItemId,
        CancellationToken cancellationToken = default)
    {
        if (workItemId <= 0)
        {
            return Array.Empty<ExperimentDto>();
        }

        return await _experimentRepository.GetByWorkItemIdAsync(
            workItemId,
            cancellationToken);
    }

    public async Task<ExperimentDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return null;
        }

        return await _experimentRepository.GetByIdAsync(
            id,
            cancellationToken);
    }

    public async Task<ExperimentDto?> CreateAsync(
        int workItemId,
        string title,
        string? description,
        string? protocol,
        DateTime? startedAt,
        DateTime? completedAt,
        string? notes,
        CancellationToken cancellationToken = default)
    {
        if (workItemId <= 0)
        {
            throw new ArgumentException(
                "WorkItemId must be greater than zero.",
                nameof(workItemId));
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException(
                "Experiment title is required.",
                nameof(title));
        }

        return await _experimentRepository.CreateAsync(
            workItemId,
            title.Trim(),
            description,
            protocol,
            startedAt,
            completedAt,
            notes,
            cancellationToken);
    }

    public async Task<bool> UpdateAsync(
        int id,
        string title,
        string? description,
        string? protocol,
        DateTime? startedAt,
        DateTime? completedAt,
        string? notes,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException(
                "Experiment title is required.",
                nameof(title));
        }

        return await _experimentRepository.UpdateAsync(
            id,
            title.Trim(),
            description,
            protocol,
            startedAt,
            completedAt,
            notes,
            cancellationToken);
    }
}