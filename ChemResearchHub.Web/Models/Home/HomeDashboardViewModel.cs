namespace ChemResearchHub.Web.Models.Home;

public class HomeDashboardViewModel
{
    public string DisplayName { get; set; } = string.Empty;

    public string RoleName { get; set; } = string.Empty;

    public string DateText { get; set; } = string.Empty;

    public int ProjectCount { get; set; }

    public int WorkItemCount { get; set; }

    public int OpenWorkItemCount { get; set; }

    public int MyOpenWorkItemCount { get; set; }

    public int ExperimentCount { get; set; }

    public int ActiveExperimentCount { get; set; }

    public int SampleCount { get; set; }

    public int ResultCount { get; set; }

    public int AttachmentCount { get; set; }

    public int DecisionLogCount { get; set; }

    public IReadOnlyList<RecentDecisionViewModel> RecentDecisions { get; set; }
        = Array.Empty<RecentDecisionViewModel>();
}

public class RecentDecisionViewModel
{
    public int Id { get; set; }

    public string DecisionType { get; set; } = string.Empty;

    public string Decision { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}