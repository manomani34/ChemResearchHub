namespace ChemResearchHub.Web.Models.Samples;

public class SampleTimelineItemViewModel
{
    public string EventType { get; init; } = string.Empty;

    public string ReferenceCode { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public string? Description { get; init; }

    public DateTime? OccurredAt { get; init; }

    public string Icon { get; init; } = "•";
}

