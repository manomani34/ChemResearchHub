namespace ChemResearchHub.Web.Areas.Admin.Models;

public class ReportsViewModel
{
    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public int? ProjectId { get; set; }

    public string? UserId { get; set; }

    public string? Status { get; set; }

    public string ReportType { get; set; } = "overview";

    public IReadOnlyList<ReportProjectItem> Projects { get; set; }
        = Array.Empty<ReportProjectItem>();

    public IReadOnlyList<ReportUserItem> Users { get; set; }
        = Array.Empty<ReportUserItem>();

    public ReportSummary Summary { get; set; } = new();

    public IReadOnlyList<ReportTableRow> Rows { get; set; }
        = Array.Empty<ReportTableRow>();
}


public class ReportSummary
{
    public int TotalProjects { get; set; }

    public int ActiveProjects { get; set; }

    public int TotalWorkItems { get; set; }

    public int OpenWorkItems { get; set; }

    public int CompletedWorkItems { get; set; }

    public int OverdueWorkItems { get; set; }

    public int TotalExperiments { get; set; }

    public int ActiveExperiments { get; set; }

    public int TotalSamples { get; set; }

    public int TotalResults { get; set; }

    public int TotalAttachments { get; set; }

    public int TotalDecisions { get; set; }
}


public class ReportProjectItem
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public int WorkItemCount { get; set; }

    public int OpenWorkItemCount { get; set; }

    public int CompletedWorkItemCount { get; set; }

    public int ExperimentCount { get; set; }
}


public class ReportUserItem
{
    public string Id { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string RoleName { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public int WorkItemCount { get; set; }

    public int OpenWorkItemCount { get; set; }

    public int CompletedWorkItemCount { get; set; }

    public int OverdueWorkItemCount { get; set; }
}


public class ReportTableRow
{
    public string Category { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public int Total { get; set; }

    public int Open { get; set; }

    public int Completed { get; set; }

    public int Overdue { get; set; }

    public DateTime? LastActivityAt { get; set; }
}