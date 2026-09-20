using ChemResearchHub.Application.Projects.Dtos;

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
        CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(
        int id,
        string name,
        string? description,
        CancellationToken cancellationToken = default);

    Task<bool> SetActiveAsync(
        int id,
        bool isActive,
        CancellationToken cancellationToken = default);
}