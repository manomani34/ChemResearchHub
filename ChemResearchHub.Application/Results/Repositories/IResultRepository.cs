using ChemResearchHub.Application.Results.Dtos;

namespace ChemResearchHub.Application.Results.Repositories;

public interface IResultRepository
{
    Task<IReadOnlyList<ResultDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ResultDto>> GetBySampleIdAsync(
        int sampleId,
        CancellationToken cancellationToken = default);

    Task<ResultDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<ResultDto?> CreateAsync(
        int sampleId,
        string metricName,
        decimal? numericValue,
        string? textValue,
        string? unit,
        string? method,
        string status,
        string? evidence,
        string? notes,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(
        int id,
        string metricName,
        decimal? numericValue,
        string? textValue,
        string? unit,
        string? method,
        string status,
        string? evidence,
        string? notes,
        CancellationToken cancellationToken = default);
}