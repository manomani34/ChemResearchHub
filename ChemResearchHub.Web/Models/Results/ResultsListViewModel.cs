namespace ChemResearchHub.Web.Models.Results;

public class ResultsListViewModel
{
    public IReadOnlyList<ResultListItemViewModel> Items { get; init; }
        = Array.Empty<ResultListItemViewModel>();
}

public class ResultListItemViewModel
{
    public int Id { get; init; }

    public int SampleId { get; init; }

    public string SampleCode { get; init; } = string.Empty;

    public string? SampleName { get; init; }

    public int ExperimentId { get; init; }

    public string ExperimentTitle { get; init; } = string.Empty;

    public int WorkItemId { get; init; }

    public string WorkItemTitle { get; init; } = string.Empty;

    public string MetricName { get; init; } = string.Empty;

    public decimal? NumericValue { get; init; }

    public string? TextValue { get; init; }

    public string? Unit { get; init; }

    public string? Method { get; init; }

    public string Status { get; init; } = "Pending";
}