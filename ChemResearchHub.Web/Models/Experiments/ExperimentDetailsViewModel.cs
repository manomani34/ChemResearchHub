namespace ChemResearchHub.Web.Models.Experiments;

public class ExperimentDetailsViewModel
{
    public int Id { get; init; }

    public string ReferenceCode { get; init; } = string.Empty;

    public int WorkItemId { get; init; }

    public string WorkItemReferenceCode { get; init; } = string.Empty;

    public string WorkItemTitle { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public string? Description { get; init; }

    public string? Protocol { get; init; }

    public DateTime? StartedAt { get; init; }

    public DateTime? CompletedAt { get; init; }

    public string? Notes { get; init; }

    public IReadOnlyList<ExperimentSampleViewModel> Samples { get; init; }
        = Array.Empty<ExperimentSampleViewModel>();

    public IReadOnlyList<ExperimentReviewViewModel> Reviews { get; init; }
        = Array.Empty<ExperimentReviewViewModel>();

    public IReadOnlyList<ExperimentDecisionViewModel> Decisions { get; init; }
        = Array.Empty<ExperimentDecisionViewModel>();

    public IReadOnlyList<ExperimentTimelineItemViewModel> Timeline { get; init; }
        = Array.Empty<ExperimentTimelineItemViewModel>();
}


public class ExperimentSampleViewModel
{
    public int Id { get; init; }

    public string ReferenceCode { get; init; } = string.Empty;

    public string SampleCode { get; init; } = string.Empty;

    public string? Name { get; init; }

    public string? SampleType { get; init; }

    public string? Matrix { get; init; }

    public DateTime? CollectedAt { get; init; }

    public IReadOnlyList<ExperimentResultViewModel> Results { get; init; }
        = Array.Empty<ExperimentResultViewModel>();
}


public class ExperimentResultViewModel
{
    public int Id { get; init; }

    public string ReferenceCode { get; init; } = string.Empty;

    public string MetricName { get; init; } = string.Empty;

    public decimal? NumericValue { get; init; }

    public string? TextValue { get; init; }

    public string? Unit { get; init; }

    public string? Method { get; init; }

    public string Status { get; init; } = "Pending";

    public string? Evidence { get; init; }

    public string? Notes { get; init; }
}


public class ExperimentReviewViewModel
{
    public int Id { get; init; }

    public string ReferenceCode { get; init; } = string.Empty;

    public string RequestedByUserName { get; init; } = string.Empty;

    public string? ReviewerUserName { get; init; }

    public string Status { get; init; } = string.Empty;

    public DateTime RequestedAtUtc { get; init; }

    public DateTime? StartedAtUtc { get; init; }

    public DateTime? ReviewedAtUtc { get; init; }

    public string? ReviewNote { get; init; }
}


public class ExperimentDecisionViewModel
{
    public int Id { get; init; }

    public string ReferenceCode { get; init; } = string.Empty;

    public string DecisionType { get; init; } = string.Empty;

    public string Decision { get; init; } = string.Empty;

    public string? Rationale { get; init; }

    public string? Evidence { get; init; }

    public DateTime CreatedAt { get; init; }
}


public class ExperimentTimelineItemViewModel
{
    public string EventType { get; init; } = string.Empty;

    public string ReferenceCode { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public string? Description { get; init; }

    public DateTime? OccurredAt { get; init; }

    public string Icon { get; init; } = "•";
}