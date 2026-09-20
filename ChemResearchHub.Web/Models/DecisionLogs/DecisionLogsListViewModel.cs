namespace ChemResearchHub.Web.Models.DecisionLogs;

public class DecisionLogsListViewModel
{
    public IReadOnlyList<DecisionLogListItemViewModel> Items { get; init; }
        = Array.Empty<DecisionLogListItemViewModel>();
}

public class DecisionLogListItemViewModel
{
    public int Id { get; init; }

    public int WorkItemId { get; init; }

    public string WorkItemTitle { get; init; } = string.Empty;

    public string DecisionType { get; init; } = string.Empty;

    public string Decision { get; init; } = string.Empty;

    public string? Rationale { get; init; }

    public string? Evidence { get; init; }

    public string? CreatedByUserId { get; init; }

    public DateTime CreatedAt { get; init; }
}