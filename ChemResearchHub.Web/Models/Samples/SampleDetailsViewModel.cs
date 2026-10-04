namespace ChemResearchHub.Web.Models.Samples;

public class SampleDetailsViewModel
{
    public int Id { get; set; }

    public string ReferenceCode { get; set; } = string.Empty;

    public string SampleCode { get; set; } = string.Empty;

    public string? Name { get; set; }

    public string? SampleType { get; set; }

    public string? Matrix { get; set; }

    public string? PreparationMethod { get; set; }

    public DateTime? CollectedAt { get; set; }

    public string? ExternalReference { get; set; }

    public string? Description { get; set; }

    public string? Notes { get; set; }

    public int ExperimentId { get; set; }

    public string ExperimentTitle { get; set; } = string.Empty;

    public int WorkItemId { get; set; }

    public string WorkItemReferenceCode { get; set; } = string.Empty;

    public string WorkItemTitle { get; set; } = string.Empty;

    public int ProjectId { get; set; }

    public string ProjectName { get; set; } = string.Empty;
    public IReadOnlyList<SampleTimelineItemViewModel> Timeline { get; set; }
    = Array.Empty<SampleTimelineItemViewModel>();

}