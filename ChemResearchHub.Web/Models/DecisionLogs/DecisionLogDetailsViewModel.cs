namespace ChemResearchHub.Web.Models.DecisionLogs;

public class DecisionLogDetailsViewModel
{
    public int Id { get; init; }

    public string ReferenceCode { get; init; } = string.Empty;

    public int WorkItemId { get; init; }

    public string WorkItemReferenceCode { get; init; } = string.Empty;

    public string WorkItemTitle { get; init; } = string.Empty;

    public int ProjectId { get; init; }

    public int BoardId { get; init; }

    public string DecisionType { get; init; } = string.Empty;

    public string Decision { get; init; } = string.Empty;

    public string? Rationale { get; init; }

    public string? Evidence { get; init; }

    public string? CreatedByUserId { get; init; }

    public DateTime CreatedAt { get; init; }
}