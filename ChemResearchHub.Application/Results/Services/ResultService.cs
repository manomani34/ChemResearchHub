using ChemResearchHub.Application.Results.Dtos;
using ChemResearchHub.Application.Results.Interfaces;
using ChemResearchHub.Application.Results.Repositories;

namespace ChemResearchHub.Application.Results.Services;

public class ResultService : IResultService
{
    private readonly IResultRepository _resultRepository;

    public ResultService(
        IResultRepository resultRepository)
    {
        _resultRepository = resultRepository;
    }

    public async Task<IReadOnlyList<ResultDto>> GetAllAsync(
    CancellationToken cancellationToken = default)
    {
        return await _resultRepository.GetAllAsync(
            cancellationToken);
    }

    public async Task<IReadOnlyList<ResultDto>> GetBySampleIdAsync(
        int sampleId,
        CancellationToken cancellationToken = default)
    {
        if (sampleId <= 0)
        {
            return Array.Empty<ResultDto>();
        }

        return await _resultRepository.GetBySampleIdAsync(
            sampleId,
            cancellationToken);
    }

    public async Task<ResultDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return null;
        }

        return await _resultRepository.GetByIdAsync(
            id,
            cancellationToken);
    }

    public async Task<ResultDto?> CreateAsync(
        int sampleId,
        string metricName,
        decimal? numericValue,
        string? textValue,
        string? unit,
        string? method,
        string status,
        string? evidence,
        string? notes,
        CancellationToken cancellationToken = default)
    {
        if (sampleId <= 0)
        {
            throw new ArgumentException(
                "SampleId must be greater than zero.",
                nameof(sampleId));
        }

        if (string.IsNullOrWhiteSpace(metricName))
        {
            throw new ArgumentException(
                "Metric name is required.",
                nameof(metricName));
        }

        return await _resultRepository.CreateAsync(
            sampleId,
            metricName.Trim(),
            numericValue,
            textValue,
            unit,
            method,
            string.IsNullOrWhiteSpace(status)
                ? "Pending"
                : status.Trim(),
            evidence,
            notes,
            cancellationToken);
    }

    public async Task<bool> UpdateAsync(
        int id,
        string metricName,
        decimal? numericValue,
        string? textValue,
        string? unit,
        string? method,
        string status,
        string? evidence,
        string? notes,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(metricName))
        {
            throw new ArgumentException(
                "Metric name is required.",
                nameof(metricName));
        }

        return await _resultRepository.UpdateAsync(
            id,
            metricName.Trim(),
            numericValue,
            textValue,
            unit,
            method,
            string.IsNullOrWhiteSpace(status)
                ? "Pending"
                : status.Trim(),
            evidence,
            notes,
            cancellationToken);
    }
}