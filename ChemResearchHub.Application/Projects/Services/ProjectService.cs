using ChemResearchHub.Application.Projects.Dtos;
using ChemResearchHub.Application.Projects.Interfaces;
using ChemResearchHub.Application.Projects.Repositories;
using ChemResearchHub.Domain.Entities.Project;

namespace ChemResearchHub.Application.Projects.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;

    public ProjectService(
        IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<IReadOnlyList<ProjectDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var projects =
            await _projectRepository.GetAllAsync(
                cancellationToken);

        return projects
            .Select(MapToDto)
            .ToList();
    }

    public async Task<ProjectDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var project =
            await _projectRepository.GetByIdAsync(
                id,
                cancellationToken);

        return project is null
            ? null
            : MapToDto(project);
    }

    public async Task<ProjectDto> CreateAsync(
        string name,
        string? description,
        CancellationToken cancellationToken = default)
    {
        var project = new Project(
            name,
            description);

        await _projectRepository.AddAsync(
            project,
            cancellationToken);

        await _projectRepository.SaveChangesAsync(
            cancellationToken);

        return MapToDto(project);
    }

    public async Task<bool> UpdateAsync(
        int id,
        string name,
        string? description,
        CancellationToken cancellationToken = default)
    {
        var project =
            await _projectRepository.GetByIdAsync(
                id,
                cancellationToken);

        if (project is null)
        {
            return false;
        }

        project.Update(
            name,
            description);

        await _projectRepository.SaveChangesAsync(
            cancellationToken);

        return true;
    }

    public async Task<bool> SetActiveAsync(
        int id,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var project =
            await _projectRepository.GetByIdAsync(
                id,
                cancellationToken);

        if (project is null)
        {
            return false;
        }

        if (isActive)
        {
            project.Activate();
        }
        else
        {
            project.Deactivate();
        }

        await _projectRepository.SaveChangesAsync(
            cancellationToken);

        return true;
    }

    private static ProjectDto MapToDto(
        Project project)
    {
        return new ProjectDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            IsActive = project.IsActive,
            CreatedAt = project.CreatedAt,
            ModifiedAt = project.ModifiedAt
        };
    }
}