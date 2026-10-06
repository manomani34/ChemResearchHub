using ChemResearchHub.Application.Experiments.Dtos;
using ChemResearchHub.Application.Experiments.Repositories;
using ChemResearchHub.Domain.Entities.Experiment;
using Microsoft.EntityFrameworkCore;

namespace ChemResearchHub.Infrastructure.Persistence.Repositories;

public class ExperimentRepository : IExperimentRepository
{
    private readonly ApplicationDbContext _context;

    public ExperimentRepository(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ExperimentStatistics> GetStatisticsByProjectIdAsync(
    int projectId,
    CancellationToken cancellationToken = default)
    {
        var experiments = _context.Experiments
            .AsNoTracking()
            .Where(x =>
                _context.WorkItems.Any(
                    w =>
                        w.Id == x.WorkItemId &&
                        w.ProjectId == projectId));

        var total = await experiments.CountAsync(
            cancellationToken);

        var completed = await experiments.CountAsync(
            x => x.CompletedAt.HasValue,
            cancellationToken);

        var started = await experiments.CountAsync(
            x => x.StartedAt.HasValue,
            cancellationToken);

        return new ExperimentStatistics
        {
            Total = total,
            Completed = completed,
            InProgress = started - completed,
            NotStarted = total - started
        };
    }

    public async Task<ExperimentStatistics> GetStatisticsAsync(
    CancellationToken cancellationToken = default)
    {
        var total = await _context.Experiments
            .CountAsync(cancellationToken);

        var completed = await _context.Experiments
            .CountAsync(
                x => x.CompletedAt.HasValue,
                cancellationToken);

        var started = await _context.Experiments
            .CountAsync(
                x => x.StartedAt.HasValue,
                cancellationToken);

        return new ExperimentStatistics
        {
            Total = total,
            Completed = completed,
            InProgress = started - completed,
            NotStarted = total - started
        };
    }

    public async Task<IReadOnlyList<ExperimentDto>> SearchAsync(
    string searchTerm,
    CancellationToken cancellationToken = default)
    {
        return await _context.Experiments
            .AsNoTracking()
            .Where(x =>
                x.Title.Contains(searchTerm) ||
                (x.Description != null &&
                 x.Description.Contains(searchTerm)) ||
                (x.Protocol != null &&
                 x.Protocol.Contains(searchTerm)) ||
                (x.Notes != null &&
                 x.Notes.Contains(searchTerm)))
            .OrderByDescending(x => x.StartedAt)
            .ThenByDescending(x => x.Id)
            .Select(x => new ExperimentDto
            {
                Id = x.Id,
                WorkItemId = x.WorkItemId,
                Title = x.Title,
                Description = x.Description,
                Protocol = x.Protocol,
                StartedAt = x.StartedAt,
                CompletedAt = x.CompletedAt,
                Notes = x.Notes
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ExperimentDto?> GetByTitleAsync(
    string title,
    CancellationToken cancellationToken = default)
    {
        return await _context.Experiments
            .AsNoTracking()
            .Where(x => x.Title == title)
            .Select(x => new ExperimentDto
            {
                Id = x.Id,
                WorkItemId = x.WorkItemId,
                Title = x.Title,
                Description = x.Description,
                Protocol = x.Protocol,
                StartedAt = x.StartedAt,
                CompletedAt = x.CompletedAt,
                Notes = x.Notes
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ExperimentDto?> GetLatestAsync(
    CancellationToken cancellationToken = default)
    {
        return await _context.Experiments
            .AsNoTracking()
            .OrderByDescending(x => x.StartedAt)
            .ThenByDescending(x => x.Id)
            .Select(x => new ExperimentDto
            {
                Id = x.Id,
                WorkItemId = x.WorkItemId,
                Title = x.Title,
                Description = x.Description,
                Protocol = x.Protocol,
                StartedAt = x.StartedAt,
                CompletedAt = x.CompletedAt,
                Notes = x.Notes
            })
            .FirstOrDefaultAsync(cancellationToken);
    }


    public async Task<IReadOnlyList<ExperimentDto>> GetByProjectIdAsync(
    int projectId,
    CancellationToken cancellationToken = default)
    {
        return await _context.Experiments
            .AsNoTracking()
            .Where(x =>
                _context.WorkItems.Any(
                    w =>
                        w.Id == x.WorkItemId &&
                        w.ProjectId == projectId))
            .OrderByDescending(x => x.StartedAt)
            .ThenByDescending(x => x.Id)
            .Select(x => new ExperimentDto
            {
                Id = x.Id,
                WorkItemId = x.WorkItemId,
                Title = x.Title,
                Description = x.Description,
                Protocol = x.Protocol,
                StartedAt = x.StartedAt,
                CompletedAt = x.CompletedAt,
                Notes = x.Notes
            })
            .ToListAsync(cancellationToken);
    }


    public async Task<IReadOnlyList<ExperimentDto>> GetAllAsync(
    CancellationToken cancellationToken = default)
    {
        return await _context.Experiments
            .AsNoTracking()
            .OrderByDescending(x => x.StartedAt)
            .ThenByDescending(x => x.Id)
            .Select(x => new ExperimentDto
            {
                Id = x.Id,
                WorkItemId = x.WorkItemId,
                Title = x.Title,
                Description = x.Description,
                Protocol = x.Protocol,
                StartedAt = x.StartedAt,
                CompletedAt = x.CompletedAt,
                Notes = x.Notes
            })
            .ToListAsync(cancellationToken);
    }


    public async Task<IReadOnlyList<ExperimentDto>> GetByWorkItemIdAsync(
        int workItemId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Experiments
            .AsNoTracking()
            .Where(x => x.WorkItemId == workItemId)
            .OrderByDescending(x => x.StartedAt)
            .ThenByDescending(x => x.Id)
            .Select(x => new ExperimentDto
            {
                Id = x.Id,
                WorkItemId = x.WorkItemId,
                Title = x.Title,
                Description = x.Description,
                Protocol = x.Protocol,
                StartedAt = x.StartedAt,
                CompletedAt = x.CompletedAt,
                Notes = x.Notes
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ExperimentDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Experiments
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new ExperimentDto
            {
                Id = x.Id,
                WorkItemId = x.WorkItemId,
                Title = x.Title,
                Description = x.Description,
                Protocol = x.Protocol,
                StartedAt = x.StartedAt,
                CompletedAt = x.CompletedAt,
                Notes = x.Notes
            })
            .FirstOrDefaultAsync(cancellationToken);
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
        var experiment =
            new Experiment(
                workItemId,
                title);

        experiment.Update(
            title,
            description,
            protocol,
            startedAt,
            completedAt,
            notes);

        await _context.Experiments.AddAsync(
            experiment,
            cancellationToken);

        await _context.SaveChangesAsync(
            cancellationToken);

        return new ExperimentDto
        {
            Id = experiment.Id,
            WorkItemId = experiment.WorkItemId,
            Title = experiment.Title,
            Description = experiment.Description,
            Protocol = experiment.Protocol,
            StartedAt = experiment.StartedAt,
            CompletedAt = experiment.CompletedAt,
            Notes = experiment.Notes
        };
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
        var experiment =
            await _context.Experiments
                .FirstOrDefaultAsync(
                    x => x.Id == id,
                    cancellationToken);

        if (experiment is null)
        {
            return false;
        }

        experiment.Update(
            title,
            description,
            protocol,
            startedAt,
            completedAt,
            notes);

        await _context.SaveChangesAsync(
            cancellationToken);

        return true;
    }
}