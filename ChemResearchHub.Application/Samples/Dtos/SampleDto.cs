namespace ChemResearchHub.Application.Samples.Dtos;

public class SampleDto
{
    public int Id { get; init; }

    public int ExperimentId { get; init; }

    public string SampleCode { get; init; } = string.Empty;

    public string? Name { get; init; }

    public string? SampleType { get; init; }

    public string? Matrix { get; init; }

    public string? PreparationMethod { get; init; }

    public DateTime? CollectedAt { get; init; }

    public string? ExternalReference { get; init; }

    public string? Description { get; init; }

    public string? Notes { get; init; }
}