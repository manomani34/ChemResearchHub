using ClosedXML.Excel;
using ChemResearchHub.Application.Attachments.Interfaces;
using ChemResearchHub.Application.DecisionLogs.Interfaces;
using ChemResearchHub.Application.Experiments.Interfaces;
using ChemResearchHub.Application.Projects.Interfaces;
using ChemResearchHub.Application.Results.Interfaces;
using ChemResearchHub.Application.Samples.Interfaces;
using ChemResearchHub.Application.Users.Interfaces;
using ChemResearchHub.Application.WorkItems.Interfaces;
using ChemResearchHub.Web.Areas.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChemResearchHub.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ReportsController : Controller
{
    private readonly IProjectService _projectService;
    private readonly IWorkItemService _workItemService;
    private readonly IUserService _userService;
    private readonly IExperimentService _experimentService;
    private readonly ISampleService _sampleService;
    private readonly IResultService _resultService;
    private readonly IAttachmentService _attachmentService;
    private readonly IDecisionLogService _decisionLogService;

    public ReportsController(
        IProjectService projectService,
        IWorkItemService workItemService,
        IUserService userService,
        IExperimentService experimentService,
        ISampleService sampleService,
        IResultService resultService,
        IAttachmentService attachmentService,
        IDecisionLogService decisionLogService)
    {
        _projectService = projectService;
        _workItemService = workItemService;
        _userService = userService;
        _experimentService = experimentService;
        _sampleService = sampleService;
        _resultService = resultService;
        _attachmentService = attachmentService;
        _decisionLogService = decisionLogService;
    }

    // =========================================================
    // Reports Page
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Index(
        ReportsViewModel model,
        CancellationToken cancellationToken)
    {
        var reportModel = await BuildReportModelAsync(
            model,
            cancellationToken);

        return View(reportModel);
    }

    // =========================================================
    // Excel Export
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> ExportExcel(
        ReportsViewModel model,
        CancellationToken cancellationToken)
    {
        var reportModel = await BuildReportModelAsync(
            model,
            cancellationToken);

        using var workbook = new XLWorkbook();

        // -----------------------------------------------------
        // Summary Sheet
        // -----------------------------------------------------

        var summarySheet = workbook.Worksheets.Add("خلاصه گزارش");

        summarySheet.RightToLeft = true;

        summarySheet.Cell(1, 1).Value = "گزارش سامانه پژوهشی ChemResearchHub";
        summarySheet.Cell(1, 1).Style.Font.Bold = true;
        summarySheet.Cell(1, 1).Style.Font.FontSize = 16;

        summarySheet.Cell(3, 1).Value = "نوع گزارش";
        summarySheet.Cell(3, 2).Value =
            GetReportTypeName(reportModel.ReportType);

        summarySheet.Cell(4, 1).Value = "از تاریخ";
        summarySheet.Cell(4, 2).Value =
            reportModel.FromDate?.ToString("yyyy-MM-dd") ?? "همه";

        summarySheet.Cell(5, 1).Value = "تا تاریخ";
        summarySheet.Cell(5, 2).Value =
            reportModel.ToDate?.ToString("yyyy-MM-dd") ?? "همه";

        summarySheet.Cell(6, 1).Value = "پروژه";
        summarySheet.Cell(6, 2).Value =
            GetSelectedProjectName(reportModel);

        summarySheet.Cell(7, 1).Value = "مسئول";
        summarySheet.Cell(7, 2).Value =
            GetSelectedUserName(reportModel);

        summarySheet.Cell(8, 1).Value = "وضعیت";
        summarySheet.Cell(8, 2).Value =
            GetStatusName(reportModel.Status);

        summarySheet.Cell(10, 1).Value = "شاخص";
        summarySheet.Cell(10, 2).Value = "مقدار";

        var summaryHeader = summarySheet.Range(10, 1, 10, 2);
        summaryHeader.Style.Font.Bold = true;

        var summaryRows = new[]
        {
            new { Name = "تعداد پروژه‌ها", Value = reportModel.Summary.TotalProjects },
            new { Name = "پروژه‌های فعال", Value = reportModel.Summary.ActiveProjects },
            new { Name = "کل Work Item ها", Value = reportModel.Summary.TotalWorkItems },
            new { Name = "Work Item های باز", Value = reportModel.Summary.OpenWorkItems },
            new { Name = "Work Item های تکمیل شده", Value = reportModel.Summary.CompletedWorkItems },
            new { Name = "Work Item های سررسید گذشته", Value = reportModel.Summary.OverdueWorkItems },
            new { Name = "کل آزمایش‌ها", Value = reportModel.Summary.TotalExperiments },
            new { Name = "آزمایش‌های فعال", Value = reportModel.Summary.ActiveExperiments },
            new { Name = "کل نمونه‌ها", Value = reportModel.Summary.TotalSamples },
            new { Name = "کل نتایج", Value = reportModel.Summary.TotalResults },
            new { Name = "کل فایل‌ها", Value = reportModel.Summary.TotalAttachments },
            new { Name = "کل تصمیم‌ها", Value = reportModel.Summary.TotalDecisions }
        };

        var summaryRow = 11;

        foreach (var item in summaryRows)
        {
            summarySheet.Cell(summaryRow, 1).Value = item.Name;
            summarySheet.Cell(summaryRow, 2).Value = item.Value;
            summaryRow++;
        }

        summarySheet.Columns().AdjustToContents();

        if (summarySheet.Column(1).Width > 35)
        {
            summarySheet.Column(1).Width = 35;
        }

        if (summarySheet.Column(2).Width > 40)
        {
            summarySheet.Column(2).Width = 40;
        }

        // -----------------------------------------------------
        // Report Sheet
        // -----------------------------------------------------

        var reportSheet = workbook.Worksheets.Add("جزئیات گزارش");

        reportSheet.RightToLeft = true;

        reportSheet.Cell(1, 1).Value =
            GetReportTypeName(reportModel.ReportType);

        reportSheet.Cell(1, 1).Style.Font.Bold = true;
        reportSheet.Cell(1, 1).Style.Font.FontSize = 15;

        var headerRow = 3;

        reportSheet.Cell(headerRow, 1).Value = "دسته";
        reportSheet.Cell(headerRow, 2).Value = "عنوان";
        reportSheet.Cell(headerRow, 3).Value = "وضعیت";
        reportSheet.Cell(headerRow, 4).Value = "کل";
        reportSheet.Cell(headerRow, 5).Value = "باز";
        reportSheet.Cell(headerRow, 6).Value = "تکمیل شده";
        reportSheet.Cell(headerRow, 7).Value = "سررسید گذشته";
        reportSheet.Cell(headerRow, 8).Value = "آخرین فعالیت";

        var reportHeader = reportSheet.Range(
            headerRow,
            1,
            headerRow,
            8);

        reportHeader.Style.Font.Bold = true;

        var currentRow = headerRow + 1;

        foreach (var row in reportModel.Rows)
        {
            reportSheet.Cell(currentRow, 1).Value = row.Category;
            reportSheet.Cell(currentRow, 2).Value = row.Name;
            reportSheet.Cell(currentRow, 3).Value = row.Status;
            reportSheet.Cell(currentRow, 4).Value = row.Total;
            reportSheet.Cell(currentRow, 5).Value = row.Open;
            reportSheet.Cell(currentRow, 6).Value = row.Completed;
            reportSheet.Cell(currentRow, 7).Value = row.Overdue;

            if (row.LastActivityAt.HasValue)
            {
                reportSheet.Cell(currentRow, 8).Value =
                    row.LastActivityAt.Value;

                reportSheet.Cell(currentRow, 8)
                    .Style
                    .DateFormat
                    .Format = "yyyy-MM-dd HH:mm";
            }

            currentRow++;
        }

        reportSheet.Columns().AdjustToContents();

        if (reportSheet.Column(2).Width > 50)
        {
            reportSheet.Column(2).Width = 50;
        }

        // -----------------------------------------------------
        // Excel Table
        // -----------------------------------------------------

        if (reportModel.Rows.Count > 0)
        {
            var lastRow = headerRow + reportModel.Rows.Count;

            var tableRange = reportSheet.Range(
                headerRow,
                1,
                lastRow,
                8);

            tableRange.CreateTable("ReportTable");
        }

        // -----------------------------------------------------
        // File
        // -----------------------------------------------------

        using var stream = new MemoryStream();

        workbook.SaveAs(stream);

        var fileName =
            $"ChemResearchHub_Report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

        return File(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }

    // =========================================================
    // Build Report Model
    // =========================================================

    private async Task<ReportsViewModel> BuildReportModelAsync(
        ReportsViewModel model,
        CancellationToken cancellationToken)
    {
        var projects = await _projectService.GetAllAsync(
            cancellationToken);

        var workItems = await _workItemService.GetAllAsync(
            cancellationToken: cancellationToken);

        var users = await _userService.GetAllAsync(
            cancellationToken);

        var experiments = await _experimentService.GetAllAsync(
            cancellationToken);

        var samples = await _sampleService.GetAllAsync(
            cancellationToken);

        var results = await _resultService.GetAllAsync(
            cancellationToken);

        var attachments = await _attachmentService.GetAllAsync(
            cancellationToken);

        var decisions = await _decisionLogService.GetAllAsync(
            cancellationToken);

        // -----------------------------------------------------
        // Work Item Filters
        // -----------------------------------------------------

        IEnumerable<
            ChemResearchHub.Application.WorkItems.Dtos.WorkItemDto>
            filteredWorkItems = workItems;

        if (model.ProjectId.HasValue)
        {
            filteredWorkItems = filteredWorkItems.Where(
                x => x.ProjectId == model.ProjectId.Value);
        }

        if (!string.IsNullOrWhiteSpace(model.UserId))
        {
            filteredWorkItems = filteredWorkItems.Where(
                x => string.Equals(
                    x.AssignedToUserId,
                    model.UserId,
                    StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(model.Status))
        {
            switch (model.Status.ToLowerInvariant())
            {
                case "open":
                    filteredWorkItems =
                        filteredWorkItems.Where(x => !x.IsCompleted);
                    break;

                case "completed":
                    filteredWorkItems =
                        filteredWorkItems.Where(x => x.IsCompleted);
                    break;

                case "overdue":
                    filteredWorkItems =
                        filteredWorkItems.Where(IsOverdue);
                    break;
            }
        }

        if (model.FromDate.HasValue)
        {
            var fromDate = model.FromDate.Value.Date;

            filteredWorkItems = filteredWorkItems.Where(
                x => x.DueDate.HasValue &&
                     x.DueDate.Value.Date >= fromDate);
        }

        if (model.ToDate.HasValue)
        {
            var toDate = model.ToDate.Value.Date;

            filteredWorkItems = filteredWorkItems.Where(
                x => x.DueDate.HasValue &&
                     x.DueDate.Value.Date <= toDate);
        }

        var filteredWorkItemList =
            filteredWorkItems.ToList();

        // -----------------------------------------------------
        // Summary
        // -----------------------------------------------------

        model.Summary = new ReportSummary
        {
            TotalProjects = projects.Count,

            ActiveProjects = projects.Count(
                x => x.IsActive),

            TotalWorkItems = filteredWorkItemList.Count,

            OpenWorkItems = filteredWorkItemList.Count(
                x => !x.IsCompleted),

            CompletedWorkItems = filteredWorkItemList.Count(
                x => x.IsCompleted),

            OverdueWorkItems = filteredWorkItemList.Count(
                IsOverdue),

            TotalExperiments = experiments.Count,

            ActiveExperiments = experiments.Count(
                x => !x.CompletedAt.HasValue &&
                     x.StartedAt.HasValue),

            TotalSamples = samples.Count,

            TotalResults = results.Count,

            TotalAttachments = attachments.Count,

            TotalDecisions = decisions.Count
        };

        // -----------------------------------------------------
        // Projects
        // -----------------------------------------------------

        model.Projects = projects
            .Select(project => new ReportProjectItem
            {
                Id = project.Id,
                Name = project.Name,
                IsActive = project.IsActive,

                WorkItemCount = filteredWorkItemList.Count(
                    x => x.ProjectId == project.Id),

                OpenWorkItemCount = filteredWorkItemList.Count(
                    x => x.ProjectId == project.Id &&
                         !x.IsCompleted),

                CompletedWorkItemCount = filteredWorkItemList.Count(
                    x => x.ProjectId == project.Id &&
                         x.IsCompleted)
            })
            .OrderBy(x => x.Name)
            .ToList();

        // -----------------------------------------------------
        // Users
        // -----------------------------------------------------

        model.Users = users
            .Select(user =>
            {
                var userWorkItems = filteredWorkItemList
                    .Where(x => string.Equals(
                        x.AssignedToUserId,
                        user.Id,
                        StringComparison.OrdinalIgnoreCase))
                    .ToList();

                return new ReportUserItem
                {
                    Id = user.Id,

                    FullName =
                        string.IsNullOrWhiteSpace(user.FullName)
                            ? user.Email ?? user.Id
                            : user.FullName,

                    Email = user.Email ?? string.Empty,

                    RoleName = user.RoleName ?? string.Empty,

                    IsActive = user.IsActive,

                    WorkItemCount = userWorkItems.Count,

                    OpenWorkItemCount =
                        userWorkItems.Count(
                            x => !x.IsCompleted),

                    CompletedWorkItemCount =
                        userWorkItems.Count(
                            x => x.IsCompleted),

                    OverdueWorkItemCount =
                        userWorkItems.Count(IsOverdue)
                };
            })
            .OrderByDescending(x => x.WorkItemCount)
            .ThenBy(x => x.FullName)
            .ToList();

        // -----------------------------------------------------
        // Report Rows
        // -----------------------------------------------------

        model.Rows = BuildRows(
            model.ReportType,
            projects,
            filteredWorkItemList,
            model.Users,
            experiments,
            samples,
            results,
            attachments,
            decisions);

        return model;
    }

    // =========================================================
    // Build Rows
    // =========================================================

    private static IReadOnlyList<ReportTableRow> BuildRows(
        string? reportType,
        IReadOnlyList<
            ChemResearchHub.Application.Projects.Dtos.ProjectDto>
            projects,

        IReadOnlyList<
            ChemResearchHub.Application.WorkItems.Dtos.WorkItemDto>
            workItems,

        IReadOnlyList<ReportUserItem> users,

        IReadOnlyList<
            ChemResearchHub.Application.Experiments.Dtos.ExperimentDto>
            experiments,

        IReadOnlyList<
            ChemResearchHub.Application.Samples.Dtos.SampleDto>
            samples,

        IReadOnlyList<
            ChemResearchHub.Application.Results.Dtos.ResultDto>
            results,

        IReadOnlyList<
            ChemResearchHub.Application.Attachments.Dtos.AttachmentDto>
            attachments,

        IReadOnlyList<
            ChemResearchHub.Application.DecisionLogs.Dtos.DecisionLogDto>
            decisions)
    {
        return reportType?.ToLowerInvariant() switch
        {
            "projects" => projects
                .Select(x => new ReportTableRow
                {
                    Category = "پروژه",
                    Name = x.Name,
                    Status = x.IsActive
                        ? "فعال"
                        : "غیرفعال",

                    Total = 1
                })
                .ToList(),

            "users" => users
                .Select(x => new ReportTableRow
                {
                    Category = "کاربر",
                    Name = x.FullName,

                    Status = x.IsActive
                        ? "فعال"
                        : "غیرفعال",

                    Total = x.WorkItemCount,
                    Open = x.OpenWorkItemCount,
                    Completed = x.CompletedWorkItemCount,
                    Overdue = x.OverdueWorkItemCount
                })
                .ToList(),

            "workitems" => workItems
                .Select(x => new ReportTableRow
                {
                    Category = "Work Item",
                    Name = x.Title,

                    Status = x.IsCompleted
                        ? "تکمیل شده"
                        : IsOverdue(x)
                            ? "سررسید گذشته"
                            : "باز",

                    Total = 1,

                    Open = x.IsCompleted
                        ? 0
                        : 1,

                    Completed = x.IsCompleted
                        ? 1
                        : 0,

                    Overdue = IsOverdue(x)
                        ? 1
                        : 0,

                    LastActivityAt = x.DueDate
                })
                .ToList(),

            "experiments" => experiments
                .Select(x => new ReportTableRow
                {
                    Category = "آزمایش",
                    Name = x.Title,

                    Status = x.CompletedAt.HasValue
                        ? "تکمیل شده"
                        : x.StartedAt.HasValue
                            ? "فعال"
                            : "شروع نشده",

                    Total = 1,

                    LastActivityAt =
                        x.CompletedAt ??
                        x.StartedAt
                })
                .ToList(),

            "samples" => samples
                .Select(x => new ReportTableRow
                {
                    Category = "نمونه",
                    Name = x.SampleCode,

                    Status = "ثبت شده",

                    Total = 1,

                    LastActivityAt = x.CollectedAt
                })
                .ToList(),

            "results" => results
                .Select(x => new ReportTableRow
                {
                    Category = "Result",
                    Name = x.MetricName,

                    Status = x.Status,

                    Total = 1
                })
                .ToList(),

            "attachments" => attachments
                .Select(x => new ReportTableRow
                {
                    Category = "Attachment",
                    Name = x.OriginalFileName,

                    Status = "ثبت شده",

                    Total = 1
                })
                .ToList(),

            "decisions" => decisions
                .Select(x => new ReportTableRow
                {
                    Category = "Decision",
                    Name = x.DecisionType,

                    Status = "ثبت شده",

                    Total = 1,

                    LastActivityAt = x.CreatedAt
                })
                .ToList(),

            _ => Array.Empty<ReportTableRow>()
        };
    }

    // =========================================================
    // Helpers
    // =========================================================

    private static bool IsOverdue(
        ChemResearchHub.Application.WorkItems.Dtos.WorkItemDto workItem)
    {
        return !workItem.IsCompleted &&
               workItem.DueDate.HasValue &&
               workItem.DueDate.Value < DateTime.UtcNow;
    }

    private static string GetReportTypeName(
        string? reportType)
    {
        return reportType?.ToLowerInvariant() switch
        {
            "projects" => "گزارش پروژه‌ها",
            "workitems" => "گزارش Work Item ها",
            "users" => "گزارش عملکرد کاربران",
            "experiments" => "گزارش آزمایش‌ها",
            "samples" => "گزارش نمونه‌ها",
            "results" => "گزارش نتایج",
            "attachments" => "گزارش فایل‌ها",
            "decisions" => "گزارش تصمیم‌ها",
            _ => "گزارش کلی"
        };
    }

    private static string GetStatusName(
        string? status)
    {
        return status?.ToLowerInvariant() switch
        {
            "open" => "باز",
            "completed" => "تکمیل شده",
            "overdue" => "سررسید گذشته",
            _ => "همه وضعیت‌ها"
        };
    }

    private static string GetSelectedProjectName(
        ReportsViewModel model)
    {
        if (!model.ProjectId.HasValue)
        {
            return "همه پروژه‌ها";
        }

        var project = model.Projects
            .FirstOrDefault(x => x.Id == model.ProjectId.Value);

        return project?.Name ?? $"پروژه #{model.ProjectId}";
    }

    private static string GetSelectedUserName(
        ReportsViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.UserId))
        {
            return "همه کاربران";
        }

        var user = model.Users
            .FirstOrDefault(x =>
                string.Equals(
                    x.Id,
                    model.UserId,
                    StringComparison.OrdinalIgnoreCase));

        return user?.FullName ?? model.UserId;
    }
}