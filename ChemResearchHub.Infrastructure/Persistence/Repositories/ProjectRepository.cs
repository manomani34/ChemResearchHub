using ChemResearchHub.Application.Projects.Dtos;
using ChemResearchHub.Application.Projects.Repositories;
using ChemResearchHub.Domain.Entities.Project;
using ChemResearchHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChemResearchHub.Infrastructure.Persistence.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ProjectRepository(
        ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ProjectStatistics> GetStatisticsAsync(
    CancellationToken cancellationToken = default)
    {
        var total = await _dbContext.Projects
            .CountAsync(cancellationToken);

        var active = await _dbContext.Projects
            .CountAsync(x => x.IsActive, cancellationToken);

        return new ProjectStatistics
        {
            Total = total,
            Active = active,
            Inactive = total - active
        };
    }

    public async Task<IReadOnlyList<Project>> SearchAsync(
    string searchTerm,
    CancellationToken cancellationToken = default)
    {
        return await _dbContext.Projects
            .AsNoTracking()
            .Where(x =>
                x.Name.Contains(searchTerm) ||
                (x.Description != null &&
                 x.Description.Contains(searchTerm)))
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Project?> GetByNameAsync(
    string name,
    CancellationToken cancellationToken = default)
    {
        return await _dbContext.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Name == name,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Project>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Projects
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Project?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Projects
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task AddAsync(
        Project project,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Projects.AddAsync(
            project,
            cancellationToken);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public async Task<Project?> GetLatestAsync(
    CancellationToken cancellationToken = default)
    {
        return await _dbContext.Projects
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }
}