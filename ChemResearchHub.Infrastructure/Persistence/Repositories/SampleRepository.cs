using ChemResearchHub.Application.Samples.Dtos;
using ChemResearchHub.Application.Samples.Repositories;
using ChemResearchHub.Domain.Entities.Sample;
using Microsoft.EntityFrameworkCore;

namespace ChemResearchHub.Infrastructure.Persistence.Repositories;

public class SampleRepository : ISampleRepository
{
    private readonly ApplicationDbContext _context;

    public SampleRepository(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<SampleDto>> GetAllAsync(
    CancellationToken cancellationToken = default)
    {
        return await _context.Samples
            .AsNoTracking()
            .OrderByDescending(x => x.CollectedAt)
            .ThenBy(x => x.SampleCode)
            .ThenByDescending(x => x.Id)
            .Select(x => new SampleDto
            {
                Id = x.Id,
                ExperimentId = x.ExperimentId,
                SampleCode = x.SampleCode,
                Name = x.Name,
                SampleType = x.SampleType,
                Matrix = x.Matrix,
                PreparationMethod = x.PreparationMethod,
                CollectedAt = x.CollectedAt,
                ExternalReference = x.ExternalReference,
                Description = x.Description,
                Notes = x.Notes
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SampleDto>> GetByExperimentIdAsync(
        int experimentId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Samples
            .AsNoTracking()
            .Where(x => x.ExperimentId == experimentId)
            .OrderBy(x => x.SampleCode)
            .ThenBy(x => x.Id)
            .Select(x => new SampleDto
            {
                Id = x.Id,
                ExperimentId = x.ExperimentId,
                SampleCode = x.SampleCode,
                Name = x.Name,
                SampleType = x.SampleType,
                Matrix = x.Matrix,
                PreparationMethod = x.PreparationMethod,
                CollectedAt = x.CollectedAt,
                ExternalReference = x.ExternalReference,
                Description = x.Description,
                Notes = x.Notes
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<SampleDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Samples
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new SampleDto
            {
                Id = x.Id,
                ExperimentId = x.ExperimentId,
                SampleCode = x.SampleCode,
                Name = x.Name,
                SampleType = x.SampleType,
                Matrix = x.Matrix,
                PreparationMethod = x.PreparationMethod,
                CollectedAt = x.CollectedAt,
                ExternalReference = x.ExternalReference,
                Description = x.Description,
                Notes = x.Notes
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<SampleDto?> CreateAsync(
        int experimentId,
        string sampleCode,
        string? name,
        string? sampleType,
        string? matrix,
        string? preparationMethod,
        DateTime? collectedAt,
        string? externalReference,
        string? description,
        string? notes,
        CancellationToken cancellationToken = default)
    {
        var sample =
            new Sample(
                experimentId,
                sampleCode);

        sample.Update(
            sampleCode,
            name,
            sampleType,
            matrix,
            preparationMethod,
            collectedAt,
            externalReference,
            description,
            notes);

        await _context.Samples.AddAsync(
            sample,
            cancellationToken);

        await _context.SaveChangesAsync(
            cancellationToken);

        return new SampleDto
        {
            Id = sample.Id,
            ExperimentId = sample.ExperimentId,
            SampleCode = sample.SampleCode,
            Name = sample.Name,
            SampleType = sample.SampleType,
            Matrix = sample.Matrix,
            PreparationMethod = sample.PreparationMethod,
            CollectedAt = sample.CollectedAt,
            ExternalReference = sample.ExternalReference,
            Description = sample.Description,
            Notes = sample.Notes
        };
    }

    public async Task<bool> UpdateAsync(
        int id,
        string sampleCode,
        string? name,
        string? sampleType,
        string? matrix,
        string? preparationMethod,
        DateTime? collectedAt,
        string? externalReference,
        string? description,
        string? notes,
        CancellationToken cancellationToken = default)
    {
        var sample =
            await _context.Samples
                .FirstOrDefaultAsync(
                    x => x.Id == id,
                    cancellationToken);

        if (sample is null)
        {
            return false;
        }

        sample.Update(
            sampleCode,
            name,
            sampleType,
            matrix,
            preparationMethod,
            collectedAt,
            externalReference,
            description,
            notes);

        await _context.SaveChangesAsync(
            cancellationToken);

        return true;
    }
}