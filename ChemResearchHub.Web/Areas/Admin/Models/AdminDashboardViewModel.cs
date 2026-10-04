namespace ChemResearchHub.Web.Areas.Admin.Models;

public class AdminDashboardViewModel
{
    // =========================
    // Users
    // =========================

    public int UserCount { get; set; }

    public int ActiveUserCount { get; set; }

    public int AdminCount { get; set; }

    public int ResearcherCount { get; set; }

    public int ReviewerCount { get; set; }


    // =========================
    // Projects
    // =========================

    public int ProjectCount { get; set; }

    public int ActiveProjectCount { get; set; }

    public int InactiveProjectCount { get; set; }


    // =========================
    // Work Items
    // =========================

    public int WorkItemCount { get; set; }

    public int OpenWorkItemCount { get; set; }

    public int CompletedWorkItemCount { get; set; }

    public int OverdueWorkItemCount { get; set; }

    public int UnassignedWorkItemCount { get; set; }


    // =========================
    // Experiments
    // =========================

    public int ExperimentCount { get; set; }

    public int ActiveExperimentCount { get; set; }

    public int CompletedExperimentCount { get; set; }


    // =========================
    // Samples
    // =========================

    public int SampleCount { get; set; }


    // =========================
    // Results
    // =========================

    public int ResultCount { get; set; }


    // =========================
    // Attachments
    // =========================

    public int AttachmentCount { get; set; }


    // =========================
    // Decisions
    // =========================

    public int DecisionCount { get; set; }


    // =========================
    // Reviews
    // =========================

    public int ReviewCount { get; set; }

    public int PendingReviewCount { get; set; }

    public int ApprovedReviewCount { get; set; }

    public int RejectedReviewCount { get; set; }


    // =========================
    // Chart Data
    // =========================

    public IReadOnlyList<DashboardChartItem> WorkItemStatusChart { get; set; }
        = Array.Empty<DashboardChartItem>();

    public IReadOnlyList<DashboardChartItem> WorkItemsByUserChart { get; set; }
        = Array.Empty<DashboardChartItem>();

    public IReadOnlyList<DashboardChartItem> ProjectStatusChart { get; set; }
        = Array.Empty<DashboardChartItem>();

    public IReadOnlyList<DashboardChartItem> ReviewStatusChart { get; set; }
        = Array.Empty<DashboardChartItem>();


    // =========================
    // Alerts
    // =========================

    public IReadOnlyList<AdminAlertItem> Alerts { get; set; }
        = Array.Empty<AdminAlertItem>();


    // =========================
    // Recent Activity
    // =========================

    public IReadOnlyList<AdminActivityItem> RecentActivities { get; set; }
        = Array.Empty<AdminActivityItem>();

    // =========================
    // Project Schedule
    // =========================

    public IReadOnlyList<AdminProjectScheduleItem> ProjectSchedule { get; set; }
        = Array.Empty<AdminProjectScheduleItem>();
}


public class DashboardChartItem
{
    public string Label { get; set; } = string.Empty;

    public int Value { get; set; }
}


public class AdminAlertItem
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Type { get; set; } = "info";

    public int Count { get; set; }
}


public class AdminActivityItem
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime? CreatedAt { get; set; }

    public string Type { get; set; } = "info";
}

public class AdminProjectScheduleItem
{
    public int ProjectId { get; set; }

    public string ReferenceCode { get; set; } = string.Empty;

    public string ProjectName { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string? StartDateJalali { get; set; }

    public string? EndDateJalali { get; set; }
}
