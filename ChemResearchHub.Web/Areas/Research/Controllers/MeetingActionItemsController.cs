using ChemResearchHub.Application.Meetings.DTOs;
using ChemResearchHub.Application.Meetings.Interfaces;
using ChemResearchHub.Application.Users.Interfaces;
using ChemResearchHub.Application.Projects.Interfaces;
using ChemResearchHub.Application.WorkItems.Interfaces;
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
    private readonly IWorkItemService _workItemService;

    public MeetingActionItemsController(
        IMeetingService meetingService,
        IUserService userService,
        IProjectService projectService,
        IWorkItemService workItemService)
    {
        _meetingService = meetingService;
        _userService = userService;
        _projectService = projectService;
        _workItemService = workItemService;
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

        ValidateRelatedEntities(request, model);

        if (!ModelState.IsValid)
        {
            return View(
                "~/Areas/Research/Views/MeetingActionItems/Index.cshtml",
                model);
        }

        try
        {
            var result = await _meetingService.AddActionItemAsync(
                meetingSessionId,
                request,
                cancellationToken);

            if (!result.HasValue)
                return NotFound();

            TempData["SuccessMessage"] =
                "Action Item با موفقیت ثبت شد.";

            return RedirectToAction(
                nameof(Index),
                new { meetingSessionId });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(
                "CreateRequest.WorkItemId",
                ex.Message);

            return View(
                "~/Areas/Research/Views/MeetingActionItems/Index.cshtml",
                model);
        }
    }

    [HttpPost("{actionItemId:int}/Link")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Link(
        int meetingSessionId,
        int actionItemId,
        int workItemId,
        CancellationToken cancellationToken)
    {
        if (workItemId <= 0)
        {
            TempData["ErrorMessage"] =
                "ابتدا یک Work Item معتبر انتخاب کنید.";

            return RedirectToAction(
                nameof(Index),
                new { meetingSessionId });
        }

        try
        {
            var result =
                await _meetingService.LinkActionItemToWorkItemAsync(
                    actionItemId,
                    workItemId,
                    cancellationToken);

            if (!result)
                return NotFound();

            TempData["SuccessMessage"] =
                "ارتباط Action Item با Work Item با موفقیت ثبت شد.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(
            nameof(Index),
            new { meetingSessionId });
    }

    [HttpPost("{actionItemId:int}/Unlink")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unlink(
        int meetingSessionId,
        int actionItemId,
        CancellationToken cancellationToken)
    {
        try
        {
            var result =
                await _meetingService.UnlinkActionItemFromWorkItemAsync(
                    actionItemId,
                    cancellationToken);

            if (!result)
                return NotFound();

            TempData["SuccessMessage"] =
                "ارتباط Action Item با Work Item قطع شد.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(
            nameof(Index),
            new { meetingSessionId });
    }

    private void ValidateRelatedEntities(
        CreateMeetingActionItemRequest request,
        ActionItemsViewModel model)
    {
        if (!string.IsNullOrWhiteSpace(request.AssignedToUserId) &&
            !model.Users.Any(x => x.Value == request.AssignedToUserId))
        {
            ModelState.AddModelError(
                "CreateRequest.AssignedToUserId",
                "کاربر انتخاب‌شده معتبر یا فعال نیست.");
        }

        if (request.ProjectId.HasValue &&
            !model.Projects.Any(x =>
                x.Value == request.ProjectId.Value.ToString()))
        {
            ModelState.AddModelError(
                "CreateRequest.ProjectId",
                "پروژه انتخاب‌شده معتبر یا فعال نیست.");
        }

        if (request.DecisionId.HasValue &&
            !model.Decisions.Any(x =>
                x.Value == request.DecisionId.Value.ToString()))
        {
            ModelState.AddModelError(
                "CreateRequest.DecisionId",
                "تصمیم انتخاب‌شده متعلق به این جلسه نیست.");
        }

        if (request.WorkItemId.HasValue &&
            !model.WorkItems.Any(x =>
                x.Value == request.WorkItemId.Value.ToString()))
        {
            ModelState.AddModelError(
                "CreateRequest.WorkItemId",
                "Work Item انتخاب‌شده معتبر نیست.");
        }
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

        var workItems = await _workItemService.GetAllAsync(
            cancellationToken: cancellationToken);

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
                .ToList(),

            WorkItems = workItems
                .OrderBy(x => x.Title)
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = $"#{x.Id} - {x.Title}"
                })
                .ToList()
        };
    }
}