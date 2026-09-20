namespace ChemResearchHub.Application.Results.Dtos;

public class ResultDto
{
    public int Id { get; init; }

    public int SampleId { get; init; }

    public string MetricName { get; init; } = string.Empty;

    public decimal? NumericValue { get; init; }

    public string? TextValue { get; init; }

    public string? Unit { get; init; }

    public string? Method { get; init; }

    public string Status { get; init; } = "Pending";

    public string? Evidence { get; init; }

    public string? Notes { get; init; }
}