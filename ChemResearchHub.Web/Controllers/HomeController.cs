using System.Globalization;
using ChemResearchHub.Infrastructure.Identity;
using ChemResearchHub.Infrastructure.Persistence;
using ChemResearchHub.Web.Models.Home;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChemResearchHub.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        var currentUser =
            await _userManager.GetUserAsync(User);

        if (currentUser is null)
        {
            return Challenge();
        }

        var roles =
            await _userManager.GetRolesAsync(
                currentUser);

        var roleName =
            roles.FirstOrDefault()
            ?? "User";

        var persianCalendar =
            new PersianCalendar();

        var now =
            DateTime.Now;

        var persianDate =
            $"{persianCalendar.GetYear(now)}/" +
            $"{persianCalendar.GetMonth(now):00}/" +
            $"{persianCalendar.GetDayOfMonth(now):00}";

        var projectCount =
            await _context.Projects.CountAsync(
                cancellationToken);

        var workItemCount =
            await _context.WorkItems.CountAsync(
                cancellationToken);

        var openWorkItemCount =
            await _context.WorkItems
                .CountAsync(
                    x => !x.IsCompleted,
                    cancellationToken);

        var myOpenWorkItemCount =
            await _context.WorkItems
                .CountAsync(
                    x =>
                        !x.IsCompleted &&
                        x.AssignedToUserId == currentUser.Id,
                    cancellationToken);

        var experimentCount =
            await _context.Experiments.CountAsync(
                cancellationToken);

        var activeExperimentCount =
            await _context.Experiments
                .CountAsync(
                    x =>
                        x.StartedAt.HasValue &&
                        !x.CompletedAt.HasValue,
                    cancellationToken);

        var sampleCount =
            await _context.Samples.CountAsync(
                cancellationToken);

        var resultCount =
            await _context.Results.CountAsync(
                cancellationToken);

        var attachmentCount =
            await _context.Attachments.CountAsync(
                cancellationToken);

        var decisionLogCount =
            await _context.DecisionLogs.CountAsync(
                cancellationToken);

        var recentDecisions =
            await _context.DecisionLogs
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .ThenByDescending(x => x.Id)
                .Take(5)
                .Select(x => new RecentDecisionViewModel
                {
                    Id = x.Id,
                    DecisionType = x.DecisionType,
                    Decision = x.Decision,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync(cancellationToken);

        var model =
            new HomeDashboardViewModel
            {
                DisplayName =
                    string.IsNullOrWhiteSpace(
                        currentUser.FullName)
                        ? currentUser.Email
                            ?? "کاربر"
                        : currentUser.FullName,

                RoleName = roleName,

                DateText = persianDate,

                ProjectCount = projectCount,

                WorkItemCount = workItemCount,

                OpenWorkItemCount =
                    openWorkItemCount,

                MyOpenWorkItemCount =
                    myOpenWorkItemCount,

                ExperimentCount =
                    experimentCount,

                ActiveExperimentCount =
                    activeExperimentCount,

                SampleCount =
                    sampleCount,

                ResultCount =
                    resultCount,

                AttachmentCount =
                    attachmentCount,

                DecisionLogCount =
                    decisionLogCount,

                RecentDecisions =
                    recentDecisions
            };

        return View(model);
    }

    [HttpGet]
    public IActionResult Privacy()
    {
        return View();
    }
}