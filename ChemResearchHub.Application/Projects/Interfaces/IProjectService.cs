using ChemResearchHub.Application.Projects.Dtos;
using ChemResearchHub.Domain.Entities.Project;

namespace ChemResearchHub.Application.Projects.Interfaces;

public interface IProjectService
{
    Task<IReadOnlyList<ProjectDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<ProjectDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<ProjectDto> CreateAsync(
        string name,
        string? description,
        DateTime? startDate,
        DateTime? endDate,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(
        int id,
        string name,
        string? description,
        DateTime? startDate,
        DateTime? endDate,
        CancellationToken cancellationToken = default);

    Task<bool> SetActiveAsync(
        int id,
        bool isActive,
        CancellationToken cancellationToken = default);

    Task<ProjectDto?> GetLatestAsync(
    CancellationToken cancellationToken = default);

    Task<ProjectDto?> GetByNameAsync(
    string name,
    CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProjectDto>> SearchAsync(
    string searchTerm,
    CancellationToken cancellationToken = default);

    Task<ProjectStatistics> GetStatisticsAsync(
    CancellationToken cancellationToken = default);
}
