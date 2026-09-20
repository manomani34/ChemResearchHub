using ChemResearchHub.Application.Samples.Dtos;
using ChemResearchHub.Application.Samples.Interfaces;
using ChemResearchHub.Application.Samples.Repositories;

namespace ChemResearchHub.Application.Samples.Services;

public class SampleService : ISampleService
{
    private readonly ISampleRepository _sampleRepository;

    public SampleService(
        ISampleRepository sampleRepository)
    {
        _sampleRepository = sampleRepository;
    }

    public async Task<IReadOnlyList<SampleDto>> GetAllAsync(
    CancellationToken cancellationToken = default)
    {
        return await _sampleRepository.GetAllAsync(
            cancellationToken);
    }

    public async Task<IReadOnlyList<SampleDto>> GetByExperimentIdAsync(
        int experimentId,
        CancellationToken cancellationToken = default)
    {
        if (experimentId <= 0)
        {
            return Array.Empty<SampleDto>();
        }

        return await _sampleRepository.GetByExperimentIdAsync(
            experimentId,
            cancellationToken);
    }

    public async Task<SampleDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return null;
        }

        return await _sampleRepository.GetByIdAsync(
            id,
            cancellationToken);
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
        if (experimentId <= 0)
        {
            throw new ArgumentException(
                "ExperimentId must be greater than zero.",
                nameof(experimentId));
        }

        if (string.IsNullOrWhiteSpace(sampleCode))
        {
            throw new ArgumentException(
                "Sample code is required.",
                nameof(sampleCode));
        }

        return await _sampleRepository.CreateAsync(
            experimentId,
            sampleCode.Trim(),
            name,
            sampleType,
            matrix,
            preparationMethod,
            collectedAt,
            externalReference,
            description,
            notes,
            cancellationToken);
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
        if (id <= 0)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(sampleCode))
        {
            throw new ArgumentException(
                "Sample code is required.",
                nameof(sampleCode));
        }

        return await _sampleRepository.UpdateAsync(
            id,
            sampleCode.Trim(),
            name,
            sampleType,
            matrix,
            preparationMethod,
            collectedAt,
            externalReference,
            description,
            notes,
            cancellationToken);
    }
}