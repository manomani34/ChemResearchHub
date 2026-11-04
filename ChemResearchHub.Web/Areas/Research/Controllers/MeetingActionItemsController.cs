
using ChemResearchHub.Application.Meetings.DTOs;
using ChemResearchHub.Application.Meetings.Interfaces;
using ChemResearchHub.Application.Users.Interfaces;
using ChemResearchHub.Application.Projects.Interfaces;
using ChemResearchHub.Web.Areas.Research.Models.Meetings.ActionItems;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ChemResearchHub.Web.Areas.Research.Controllers;

[Area("Research")]
[Route("Meetings/{meetingSessionId:int}/ActionItems")]
[Authorize]
public class MeetingActionItemsController : Controller
{
    private readonly IMeetingService _meetingService;
    private readonly IUserService _userService;
    private readonly IProjectService _projectService;

    public MeetingActionItemsController(
        IMeetingService meetingService,
        IUserService userService,
        IProjectService projectService)
    {
        _meetingService = meetingService;
        _userService = userService;
        _projectService = projectService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(
        int meetingSessionId,
        CancellationToken cancellationToken)
    {
        var model = await BuildViewModelAsync(
            meetingSessionId,
            null,
            cancellationToken);

        if (model is null)
            return NotFound();

        return View(
            "~/Areas/Research/Views/MeetingActionItems/Index.cshtml",
            model);
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        int meetingSessionId,
        [Bind(Prefix = "CreateRequest")]
        CreateMeetingActionItemRequest request,
        CancellationToken cancellationToken)
    {
        var model = await BuildViewModelAsync(
            meetingSessionId,
            request,
            cancellationToken);

        if (model is null)
            return NotFound();

        // اعتبارسنجی کاربر مسئول
        if (!string.IsNullOrWhiteSpace(request.AssignedToUserId) &&
            !model.Users.Any(x =>
                x.Value == request.AssignedToUserId))
        {
            ModelState.AddModelError(
                "CreateRequest.AssignedToUserId",
                "کاربر انتخاب‌شده معتبر یا فعال نیست.");
        }

        // اعتبارسنجی پروژه
        if (request.ProjectId.HasValue &&
            !model.Projects.Any(x =>
                x.Value == request.ProjectId.Value.ToString()))
        {
            ModelState.AddModelError(
                "CreateRequest.ProjectId",
                "پروژه انتخاب‌شده معتبر یا فعال نیست.");
        }

        // اعتبارسنجی تعلق تصمیم به جلسه جاری
        if (request.DecisionId.HasValue &&
            !model.Decisions.Any(x =>
                x.Value == request.DecisionId.Value.ToString()))
        {
            ModelState.AddModelError(
                "CreateRequest.DecisionId",
                "تصمیم انتخاب‌شده متعلق به این جلسه نیست.");
        }

        if (!ModelState.IsValid)
        {
            return View(
                "~/Areas/Research/Views/MeetingActionItems/Index.cshtml",
                model);
        }

        await _meetingService.AddActionItemAsync(
            meetingSessionId,
            request,
            cancellationToken);

        TempData["SuccessMessage"] =
            "Action Item با موفقیت ثبت شد.";

        return RedirectToAction(
            nameof(Index),
            new { meetingSessionId });
    }

    private async Task<ActionItemsViewModel?> BuildViewModelAsync(
        int meetingSessionId,
        CreateMeetingActionItemRequest? request,
        CancellationToken cancellationToken)
    {
        var session = await _meetingService.GetByIdAsync(
            meetingSessionId,
            cancellationToken);

        if (session is null)
            return null;

        var items = await _meetingService.GetActionItemsAsync(
            meetingSessionId,
            cancellationToken);

        var users = await _userService.GetActiveUsersAsync(
            cancellationToken);

        var projects = await _projectService.GetAllAsync(
            cancellationToken);

        var decisions = await _meetingService.GetDecisionsAsync(
            meetingSessionId,
            cancellationToken);

        return new ActionItemsViewModel
        {
            MeetingSessionId = meetingSessionId,
            Items = items,

            CreateRequest = request ?? new CreateMeetingActionItemRequest(),

            Users = users
                .Select(x => new SelectListItem
                {
                    Value = x.Id,
                    Text = string.IsNullOrWhiteSpace(x.FullName)
                        ? x.Email
                        : x.FullName
                })
                .ToList(),

            Projects = projects
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name
                })
                .ToList(),

            Decisions = decisions
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Title
                })
                .ToList()
        };
    }
}
