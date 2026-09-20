namespace ChemResearchHub.Web.Models.Samples;

public class SamplesListViewModel
{
    public IReadOnlyList<SampleListItemViewModel> Items { get; init; }
        = Array.Empty<SampleListItemViewModel>();
}

public class SampleListItemViewModel
{
    public int Id { get; init; }

    public int ExperimentId { get; init; }

    public string ExperimentTitle { get; init; } = string.Empty;

    public int WorkItemId { get; init; }

    public string WorkItemTitle { get; init; } = string.Empty;

    public string SampleCode { get; init; } = string.Empty;

    public string? Name { get; init; }

    public string? SampleType { get; init; }

    public string? Matrix { get; init; }

    public DateTime? CollectedAt { get; init; }
}