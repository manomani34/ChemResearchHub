namespace ChemResearchHub.Web.Models.Experiments;

public class ExperimentsListViewModel
{
    public IReadOnlyList<ExperimentListItemViewModel> Items { get; init; }
        = Array.Empty<ExperimentListItemViewModel>();
}

public class ExperimentListItemViewModel
{
    public int Id { get; init; }

    public int WorkItemId { get; init; }

    public string WorkItemTitle { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public DateTime? StartedAt { get; init; }

    public DateTime? CompletedAt { get; init; }

    public bool IsCompleted =>
        CompletedAt.HasValue;
}