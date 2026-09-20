using ChemResearchHub.Application.Samples.Dtos;

namespace ChemResearchHub.Application.Samples.Repositories;

public interface ISampleRepository
{
    Task<IReadOnlyList<SampleDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SampleDto>> GetByExperimentIdAsync(
        int experimentId,
        CancellationToken cancellationToken = default);

    Task<SampleDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<SampleDto?> CreateAsync(
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
        CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(
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
        CancellationToken cancellationToken = default);
}