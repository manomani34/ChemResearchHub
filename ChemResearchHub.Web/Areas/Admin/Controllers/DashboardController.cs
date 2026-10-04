using ChemResearchHub.Application.Attachments.Interfaces;
using ChemResearchHub.Application.DecisionLogs.Interfaces;
using ChemResearchHub.Application.Experiments.Interfaces;
using ChemResearchHub.Application.Projects.Interfaces;
using ChemResearchHub.Application.ResearchReviews.Interfaces;
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
public class DashboardController : Controller
{
    private readonly IProjectService _projectService;
    private readonly IWorkItemService _workItemService;
    private readonly IUserService _userService;
    private readonly IExperimentService _experimentService;
    private readonly ISampleService _sampleService;
    private readonly IResultService _resultService;
    private readonly IAttachmentService _attachmentService;
    private readonly IDecisionLogService _decisionLogService;
    private readonly IResearchReviewService _researchReviewService;

    public DashboardController(
        IProjectService projectService,
        IWorkItemService workItemService,
        IUserService userService,
        IExperimentService experimentService,
        ISampleService sampleService,
        IResultService resultService,
        IAttachmentService attachmentService,
        IDecisionLogService decisionLogService,
        IResearchReviewService researchReviewService)
    {
        _projectService = projectService;
        _workItemService = workItemService;
        _userService = userService;
        _experimentService = experimentService;
        _sampleService = sampleService;
        _resultService = resultService;
        _attachmentService = attachmentService;
        _decisionLogService = decisionLogService;
        _researchReviewService = researchReviewService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var projects = await _projectService.GetAllAsync(cancellationToken);
        var workItems = await _workItemService.GetAllAsync(cancellationToken: cancellationToken);
        var users = await _userService.GetAllAsync(cancellationToken);
        var activeUsers = await _userService.GetActiveUsersAsync(cancellationToken);
        var experiments = await _experimentService.GetAllAsync(cancellationToken);
        var samples = await _sampleService.GetAllAsync(cancellationToken);
        var results = await _resultService.GetAllAsync(cancellationToken);
        var attachments = await _attachmentService.GetAllAsync(cancellationToken);
        var decisions = await _decisionLogService.GetAllAsync(cancellationToken);

        var model = new AdminDashboardViewModel
        {
            UserCount = users.Count,
            ActiveUserCount = activeUsers.Count,
            AdminCount = users.Count(x =>
                string.Equals(x.RoleName, "Admin", StringComparison.OrdinalIgnoreCase)),
            ResearcherCount = users.Count(x =>
                string.Equals(x.RoleName, "Researcher", StringComparison.OrdinalIgnoreCase)),
            ReviewerCount = users.Count(x =>
                string.Equals(x.RoleName, "Reviewer", StringComparison.OrdinalIgnoreCase)),

            ProjectCount = projects.Count,
            ActiveProjectCount = projects.Count(x => x.IsActive),
            InactiveProjectCount = projects.Count(x => !x.IsActive),

            WorkItemCount = workItems.Count,
            CompletedWorkItemCount = workItems.Count(x => x.IsCompleted),
            OpenWorkItemCount = workItems.Count(x => !x.IsCompleted),
            OverdueWorkItemCount = workItems.Count(IsOverdue),
            UnassignedWorkItemCount = workItems.Count(
                x => string.IsNullOrWhiteSpace(x.AssignedToUserId)),

            ExperimentCount = experiments.Count,
            CompletedExperimentCount = experiments.Count(x => x.CompletedAt.HasValue),
            ActiveExperimentCount = experiments.Count(
                x => !x.CompletedAt.HasValue && x.StartedAt.HasValue),

            SampleCount = samples.Count,
            ResultCount = results.Count,
            AttachmentCount = attachments.Count,
            DecisionCount = decisions.Count
        };

        model.WorkItemStatusChart = new[]
        {
        new DashboardChartItem
        {
            Label = "باز",
            Value = model.OpenWorkItemCount
        },
        new DashboardChartItem
        {
            Label = "تکمیل شده",
            Value = model.CompletedWorkItemCount
        },
        new DashboardChartItem
        {
            Label = "سررسید گذشته",
            Value = model.OverdueWorkItemCount
        }
    };

        model.ProjectStatusChart = new[]
        {
        new DashboardChartItem
        {
            Label = "فعال",
            Value = model.ActiveProjectCount
        },
        new DashboardChartItem
        {
            Label = "غیرفعال",
            Value = model.InactiveProjectCount
        }
    };

        model.WorkItemsByUserChart =
            workItems
                .GroupBy(x =>
                    string.IsNullOrWhiteSpace(x.AssignedToUserId)
                        ? "بدون مسئول"
                        : x.AssignedToUserId)
                .Select(x => new DashboardChartItem
                {
                    Label = x.Key,
                    Value = x.Count()
                })
                .OrderByDescending(x => x.Value)
                .Take(10)
                .ToList();

        model.Alerts = BuildAlerts(model);

        model.RecentActivities =
            decisions
                .OrderByDescending(x => x.CreatedAt)
                .Take(10)
                .Select(x => new AdminActivityItem
                {
                    Title = x.DecisionType,
                    Description = x.Decision,
                    CreatedAt = x.CreatedAt,
                    Type = "decision"
                })
                .ToList();

        return View(model);
    }


    private static bool IsOverdue(
        ChemResearchHub.Application.WorkItems.Dtos.WorkItemDto workItem)
    {
        return !workItem.IsCompleted &&
               workItem.DueDate.HasValue &&
               workItem.DueDate.Value < DateTime.UtcNow;
    }

    private static IReadOnlyList<AdminAlertItem> BuildAlerts(
        AdminDashboardViewModel model)
    {
        var alerts = new List<AdminAlertItem>();

        if (model.OverdueWorkItemCount > 0)
        {
            alerts.Add(
                new AdminAlertItem
                {
                    Title = "Work Item های سررسید گذشته",
                    Description =
                        "تعدادی Work Item از زمان تعیین شده عبور کرده اند.",
                    Type = "danger",
                    Count = model.OverdueWorkItemCount
                });
        }

        if (model.UnassignedWorkItemCount > 0)
        {
            alerts.Add(
                new AdminAlertItem
                {
                    Title = "Work Item بدون مسئول",
                    Description =
                        "این Work Item ها هنوز به کاربر مشخصی اختصاص داده نشده اند.",
                    Type = "warning",
                    Count = model.UnassignedWorkItemCount
                });
        }

        if (model.InactiveProjectCount > 0)
        {
            alerts.Add(
                new AdminAlertItem
                {
                    Title = "پروژه غیرفعال",
                    Description =
                        "تعدادی پروژه در حال حاضر غیرفعال هستند.",
                    Type = "info",
                    Count = model.InactiveProjectCount
                });
        }

        return alerts;
    }
}