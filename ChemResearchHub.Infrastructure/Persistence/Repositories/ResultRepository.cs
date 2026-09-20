using ChemResearchHub.Application.Results.Dtos;
using ChemResearchHub.Application.Results.Repositories;
using ChemResearchHub.Domain.Entities.Result;
using Microsoft.EntityFrameworkCore;

namespace ChemResearchHub.Infrastructure.Persistence.Repositories;

public class ResultRepository : IResultRepository
{
    private readonly ApplicationDbContext _context;

    public ResultRepository(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ResultDto>> GetAllAsync(
    CancellationToken cancellationToken = default)
    {
        return await _context.Results
            .AsNoTracking()
            .OrderByDescending(x => x.Id)
            .Select(x => new ResultDto
            {
                Id = x.Id,
                SampleId = x.SampleId,
                MetricName = x.MetricName,
                NumericValue = x.NumericValue,
                TextValue = x.TextValue,
                Unit = x.Unit,
                Method = x.Method,
                Status = x.Status,
                Evidence = x.Evidence,
                Notes = x.Notes
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ResultDto>> GetBySampleIdAsync(
        int sampleId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Results
            .AsNoTracking()
            .Where(x => x.SampleId == sampleId)
            .OrderBy(x => x.MetricName)
            .ThenBy(x => x.Id)
            .Select(x => new ResultDto
            {
                Id = x.Id,
                SampleId = x.SampleId,
                MetricName = x.MetricName,
                NumericValue = x.NumericValue,
                TextValue = x.TextValue,
                Unit = x.Unit,
                Method = x.Method,
                Status = x.Status,
                Evidence = x.Evidence,
                Notes = x.Notes
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ResultDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Results
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new ResultDto
            {
                Id = x.Id,
                SampleId = x.SampleId,
                MetricName = x.MetricName,
                NumericValue = x.NumericValue,
                TextValue = x.TextValue,
                Unit = x.Unit,
                Method = x.Method,
                Status = x.Status,
                Evidence = x.Evidence,
                Notes = x.Notes
            })
            .FirstOrDefaultAsync(cancellationToken);
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
        var result =
            new Result(
                sampleId,
                metricName);

        result.Update(
            metricName,
            numericValue,
            textValue,
            unit,
            method,
            status,
            evidence,
            notes);

        await _context.Results.AddAsync(
            result,
            cancellationToken);

        await _context.SaveChangesAsync(
            cancellationToken);

        return new ResultDto
        {
            Id = result.Id,
            SampleId = result.SampleId,
            MetricName = result.MetricName,
            NumericValue = result.NumericValue,
            TextValue = result.TextValue,
            Unit = result.Unit,
            Method = result.Method,
            Status = result.Status,
            Evidence = result.Evidence,
            Notes = result.Notes
        };
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
        var result =
            await _context.Results
                .FirstOrDefaultAsync(
                    x => x.Id == id,
                    cancellationToken);

        if (result is null)
        {
            return false;
        }

        result.Update(
            metricName,
            numericValue,
            textValue,
            unit,
            method,
            status,
            evidence,
            notes);

        await _context.SaveChangesAsync(
            cancellationToken);

        return true;
    }
}