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