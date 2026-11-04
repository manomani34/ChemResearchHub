
using System.Security.Claims;
using ChemResearchHub.Application.Meetings.DTOs;
using ChemResearchHub.Application.Meetings.Interfaces;
using ChemResearchHub.Application.Projects.Interfaces;
using ChemResearchHub.Web.Areas.Research.Models.Meetings.ParkingLot;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ChemResearchHub.Web.Areas.Research.Controllers;

[Area("Research")]
[Route("Meetings/{meetingSessionId:int}/ParkingLot")]
[Authorize]
public class MeetingParkingLotController : Controller
{
    private readonly IMeetingService _meetingService;
    private readonly IProjectService _projectService;

    public MeetingParkingLotController(
        IMeetingService meetingService,
        IProjectService projectService)
    {
        _meetingService = meetingService;
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
            "~/Areas/Research/Views/MeetingParkingLot/Index.cshtml",
            model);
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        int meetingSessionId,
        [Bind(Prefix = "CreateRequest")]
        CreateParkingLotRequest request,
        CancellationToken cancellationToken)
    {
        var model = await BuildViewModelAsync(
            meetingSessionId,
            request,
            cancellationToken);

        if (model is null)
            return NotFound();

        // اعتبارسنجی پروژه انتخاب‌شده
        if (request.ProjectId.HasValue &&
            !model.Projects.Any(x =>
                x.Value == request.ProjectId.Value.ToString()))
        {
            ModelState.AddModelError(
                "CreateRequest.ProjectId",
                "پروژه انتخاب‌شده معتبر یا فعال نیست.");
        }

        // اعتبارسنجی تعلق دستورجلسه به جلسه جاری
        if (request.AgendaItemId.HasValue &&
            !model.AgendaItems.Any(x =>
                x.Value == request.AgendaItemId.Value.ToString()))
        {
            ModelState.AddModelError(
                "CreateRequest.AgendaItemId",
                "دستورجلسه انتخاب‌شده متعلق به این جلسه نیست.");
        }

        if (!ModelState.IsValid)
        {
            return View(
                "~/Areas/Research/Views/MeetingParkingLot/Index.cshtml",
                model);
        }

        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Forbid();

        await _meetingService.AddParkingLotItemAsync(
            meetingSessionId,
            request,
            userId,
            cancellationToken);

        TempData["SuccessMessage"] =
            "موضوع با موفقیت به Parking Lot اضافه شد.";

        return RedirectToAction(
            nameof(Index),
            new { meetingSessionId });
    }

    private async Task<ParkingLotBoardViewModel?> BuildViewModelAsync(
        int meetingSessionId,
        CreateParkingLotRequest? request,
        CancellationToken cancellationToken)
    {
        var session = await _meetingService.GetByIdAsync(
            meetingSessionId,
            cancellationToken);

        if (session is null)
            return null;

        var items = await _meetingService.GetParkingLotItemsAsync(
            meetingSessionId,
            cancellationToken);

        var projects = await _projectService.GetAllAsync(
            cancellationToken);

        return new ParkingLotBoardViewModel
        {
            MeetingSessionId = meetingSessionId,
            Items = items,

            CreateRequest = request ?? new CreateParkingLotRequest(),

            Projects = projects
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name
                })
                .ToList(),

            AgendaItems = session.AgendaItems
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Title
                })
                .ToList()
        };
    }
}
