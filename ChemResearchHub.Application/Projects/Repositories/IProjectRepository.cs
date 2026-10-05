using ChemResearchHub.Domain.Entities.Project;
using ChemResearchHub.Application.Projects.Dtos;

namespace ChemResearchHub.Application.Projects.Repositories;

public interface IProjectRepository
{
    Task<IReadOnlyList<Project>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<Project?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Project project,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);

    Task<Project?> GetLatestAsync(
   CancellationToken cancellationToken = default);

    Task<Project?> GetByNameAsync(
    string name,
    CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Project>> SearchAsync(
    string searchTerm,
    CancellationToken cancellationToken = default);

    Task<ProjectStatistics> GetStatisticsAsync(
    CancellationToken cancellationToken = default);

}